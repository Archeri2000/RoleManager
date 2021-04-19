using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.WebSocket;
using RoleManager.Model;
using RoleManager.Repository;
using static RoleManager.ReactionRoleUtils;

namespace RoleManager.Service
{
    public class ReactionRoleService
    {
        private const string SERVICE_NAME = "ReactRole";
        private readonly IRoleEventStorageRepository _eventStorage;
        private readonly SourcedLoggingService _logging;
        private readonly IDiscordLogMessageService _logMessageService;
        private readonly DiscordSocketRestClient _discordClient;

        public ReactionRoleService(IRoleEventStorageRepository eventStorage, 
            IDiscordLogMessageService logMessageService,
            DiscordSocketClient client,
            ILoggingService logging)
        {
            _eventStorage = eventStorage;
            _logMessageService = logMessageService;
            _discordClient = client.Rest;
            _logging = new SourcedLoggingService(logging, SERVICE_NAME);
        }

        public async Task<List<ReactionRoleModel>> InitialiseReactionRoles(IReactionRoleRuleRepository repo)
        {
            _logging.Info("Getting Reaction Roles from Repository...");
            var reactionRoles = await repo.GetReactionRoles();
            if (reactionRoles.IsFailure())
            {
                _logging.Fatal("Failed to retrieve Reaction Roles from Repository!", reactionRoles.FailureOrDefault());
                throw reactionRoles.FailureOrDefault();
            }

            return reactionRoles
                .Get()
                .Select(UpsertReactionMessage)
                .Where(x => !x.succeeded)
                .Select(x => x.reactionRole)
                .ToList();
        }

        public static async Task<ReactionRoleService> InitReactionRoleService(IReactionRoleRuleRepository repo,
            IRoleEventStorageRepository eventStorage, 
            IDiscordLogMessageService logMessageService,
            DiscordSocketClient client,
            ILoggingService logging)
        {
            var svc = new ReactionRoleService(eventStorage, logMessageService, client, logging);
            var failed = await svc.InitialiseReactionRoles(repo);
            failed.ForEach(x => svc._logging.Error($"Failed to initialise {x.Name} from server {x.GuildId}!"));
            return svc;
        }

        public async Task OnReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == _discordClient.CurrentUser.Id) return;
            var result =
                from guildChannel in channel.AsGuildChannel().ToAsyncResult()
                from guild in guildChannel.Guild.ToAsyncResult()
                from guildUser in _discordClient.GetGuildUser(guild.Id, reaction.UserId)
                from res in
                    TryGetGuild(guild.Id)
                        .ToAsyncResult()
                        .Then(TryGetChannel(guildChannel.Id))
                        .Then(TryGetMessage(message.Id))
                        .BranchingIfAwait(x => x is ReactionRuleModel,
                            Then:x => (x as ReactionRuleModel)
                                .ToAsyncResult()
                                .Then(TryGetAction(reaction.Emote.Name))
                                .Then(MapReactionRoleAction(guild, guildUser), Errors.MapAll)
                                .ThenAwait(RRManageRolesOnUser(guildUser), Errors.MapAll)
                                .DoAwait(PersistIfConfigured),
                            Else:x => (x as ReverseRuleModel)
                                .ToAsyncResult()
                                .Then(IsCorrectReverseReaction(reaction.Emote.Name))
                                .ThenAwait(GetRolesToRestore(guildUser, guild))
                                .ThenAwait(RRManageRolesOnUser(guildUser), Errors.MapAll))
                        .DoAwait(RemoveReactionIfConfigured(reaction, message))
                        .DoAwait(EmitLogMessageIfConfigured(guild))
                select res;
            (await result).Match(
                Success: x =>
                {
                    var (update, _) = x;
                    _logging.Info($"Successfully added [{update.RolesChanged.ToAdd.Stringify()}] " +
                                         $"and removed [{update.RolesChanged.ToRemove.Stringify()}] on {update.User.Username + update.User.DiscriminatorValue}!");
                },
                Failure: x =>
                {
                    _logging.Error($"Failed to apply reaction role: {x}");
                });
        }

        private ConcurrentDictionary<ulong, GuildReactionRole> _reactionGuilds = new();

        public (ReactionRoleModel reactionRole, bool succeeded) UpsertReactionMessage(ReactionRoleModel model)
        {
            return (model, UpsertReactionMessage(model.GuildId, model.ChannelId, model.MessageId, model.Rule));
        }

        private bool UpsertReactionMessage(ulong guildId, ulong channelId, ulong messageId, IReactionRoleModel rule)
        {
            if (!_reactionGuilds.TryGetValue(guildId, out var guild))
            {
                guild = new GuildReactionRole();
                _reactionGuilds.TryAdd(guildId, guild);
            }

            return guild.UpsertReactionMessage(channelId, messageId, rule);
        }

        private Result<GuildReactionRole> TryGetGuild(ulong guildId)
        {
            if (_reactionGuilds.TryGetValue(guildId, out var guild))
            {
                return guild;
            }

            return new KeyNotFoundException($"Guild {guildId} does not have a Reaction Role!");

        }

        private async Task<Result<Unit>> PersistIfConfigured((RoleUpdateEvent updateEvent, ReactionRoleConfig config) x)
        {
            var (updateEvent, config) = x;
            if (!config.ShouldStoreData)
            {
                return new Unit();
            }

            return await _eventStorage.Store(config.StorageKey, updateEvent);
        }

        private Func<(RoleUpdateEvent updateEvent, ReactionRoleConfig config),Task<Result<Unit>>> RemoveReactionIfConfigured(SocketReaction reaction, Cacheable<IUserMessage, ulong> backupMessage)
        {
            return async (x) =>
            {
                var (updateEvent, config) = x;
                _logging.Verbose($"Checking RemoveReaction: {config.ShouldRemoveReaction}");
                if (!config.ShouldRemoveReaction)
                {
                    return new Unit();
                }
                IUserMessage message;
                if (reaction.Message.IsSpecified)
                {
                    message = reaction.Message.Value;
                }
                else
                {
                    message = await Task.Run(backupMessage.GetOrDownloadAsync);
                }

                if (message is null)
                {
                    _logging.Error("Message is unspecified!");
                    return new Unit();
                }

                await message.RemoveReactionAsync(reaction.Emote, updateEvent.User.Id);
                return new Unit();
            };
        }

        private Func<(RoleUpdateEvent updateEvent, ReactionRoleConfig config), Task<Result<Unit>>> EmitLogMessageIfConfigured(IGuild guild)
        {
            return async x =>
            {
                var embedMessage = CreateReactionRoleEmbedLog(x.updateEvent, x.config);
                _logMessageService.WriteLogToGuild(guild, embedMessage);
                return new Unit();
            };
        }

        private Func<ReverseRuleModel, Result<ReactionRoleConfig>> IsCorrectReverseReaction(string emote)
        {
            return x => (emote == x.Emote)
                ? x.Config
                : new Exception("Reverse Reaction did not match emote!");
        }

        private Func<ReactionRoleConfig, Task<Result<(RoleManageDomain toUpdate, ReactionRoleConfig config)>>>
            GetRolesToRestore(IUser user, IGuild guild)
        {
            return async x =>
            {
                var updateEvent = await _eventStorage.Load(x.StorageKey, user.Id);
                if (updateEvent.IsFailure())
                {
                    await _logMessageService.TryAlertStaff(guild, $"User {MentionUtils.MentionUser(user.Id)} reacted to reaction role: {x.Name}, but it failed!");
                }
                return updateEvent.Then(evt => (new RoleManageDomain(evt.RolesChanged.ToRemove, evt.RolesChanged.ToAdd), x).ToResult());
            };
        }
    }

    public class GuildReactionRole
    {
        private ConcurrentDictionary<ulong, ChannelReactionRole> _reactionChannels;
        
        public GuildReactionRole(ConcurrentDictionary<ulong, ChannelReactionRole> reactionChannels)
        {
            _reactionChannels = reactionChannels;
        }

        public GuildReactionRole()
        {
            _reactionChannels = new();
        }

        public bool UpsertReactionMessage(ulong channelId, ulong messageId, IReactionRoleModel rule)
        {
            if (!_reactionChannels.TryGetValue(channelId, out var channel))
            {
                channel = new ChannelReactionRole();
                _reactionChannels.TryAdd(channelId, channel);
            }
            return channel.UpsertReactionMessage(messageId, rule);
        }

        public Result<ChannelReactionRole> TryGetChannel(ulong channelId)
        {
            if (_reactionChannels.TryGetValue(channelId, out var channel))
            {
                return channel;
            }

            return new KeyNotFoundException($"Channel {channelId} does not have a Reaction Role!");
        }
    }

    public class ChannelReactionRole
    {
        private ConcurrentDictionary<ulong, IReactionRoleModel> _reactionMessages;

        public ChannelReactionRole()
        {
            _reactionMessages = new();
        }
        public ChannelReactionRole(ConcurrentDictionary<ulong, IReactionRoleModel> reactionMessages)
        {
            _reactionMessages = reactionMessages;
        }

        public bool UpsertReactionMessage(ulong messageId, IReactionRoleModel rule)
        {
            _reactionMessages[messageId] = rule;
            return true;        
        }
        
        public Result<IReactionRoleModel> TryGetMessage(ulong messageId)
        {
            if (_reactionMessages.TryGetValue(messageId, out var rule))
            {
                return rule.ToResult();
            }

            return new KeyNotFoundException($"Message {messageId} does not have a Reaction Role!");
        }
    }
}
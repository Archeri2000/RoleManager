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
using RoleManager.Utils;
using static RoleManager.Utils.ReactionRoleUtils;

namespace RoleManager.Service
{
    public partial class ReactionRoleService
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
                                         $"and removed [{update.RolesChanged.ToRemove.Stringify()}] on { update.User.Username + update.User.DiscriminatorValue}!");
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

        private bool UpsertReactionMessage(ulong guildId, ulong channelId, ulong messageId, ReactionRuleModelBase rule)
        {
            if (!_reactionGuilds.TryGetValue(guildId, out var guild))
            {
                guild = new GuildReactionRole();
                _reactionGuilds.TryAdd(guildId, guild);
            }

            return guild.UpsertReactionMessage(channelId, messageId, rule);
        }

       
    }
}
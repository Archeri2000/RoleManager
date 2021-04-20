using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.WebSocket;
using RoleManager.Model;
using static RoleManager.Utils.ReactionRoleUtils;

namespace RoleManager.Service
{
    public partial class ReactionRoleService
    {
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

            return await _eventStorage.Store(config.StorageKey, updateEvent.ToModel());
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
                return updateEvent.Then(evt => (new RoleManageDomain(evt.RolesChanged.ToRemove.Select(guild.GetRole).ToImmutableList(), evt.RolesChanged.ToAdd.Select(guild.GetRole).ToImmutableList()), x).ToResult());
            };
        }
    }
}
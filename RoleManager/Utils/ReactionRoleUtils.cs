using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RoleManager.Model;
using RoleManager.Service;

namespace RoleManager.Utils
{
    public static class ReactionRoleUtils
    {
        public static Func<GuildReactionRole, Result<ChannelReactionRole>> TryGetChannel(ulong channelId)
        {
            return guild => guild.TryGetChannel(channelId);
        }

        public static Func<ChannelReactionRole, Result<IReactionRuleModel>> TryGetMessage(ulong messageId)
        {
            return channel => channel.TryGetMessage(messageId);
        }
        
        public static Func<ReactionRuleModel, Result<(RoleManageModel, ReactionRoleConfig)>> TryGetAction(
            string emote)
        {
            return message => message.TryGetAction(emote);
        }

        public static Result<SocketGuildChannel> AsGuildChannel(this ISocketMessageChannel channel)
        {
            if (channel is not SocketGuildChannel guildChannel)
            {
                return new InvalidCastException($"Unable to cast {channel.Name} as Guild Channel!");
            }

            return guildChannel;

        }

        public static Result<SocketGuildUser> AsGuildUser(this Optional<IUser> user)
        {
            if (!user.IsSpecified)
            {
                return new NullReferenceException("No User supplied!");
            }
            if (user.Value is not SocketGuildUser guildUser)
            {
                return new InvalidCastException($"Unable to cast {user.Value.Username} as Guild User!");
            }
            return guildUser;
        }

        public static async Task<Result<RestGuildUser>> GetGuildUser(this DiscordSocketRestClient client, ulong guildId, ulong userId)
        {
            var task = client.GetGuildUserAsync(guildId, userId);
            var result = await Task.Run(() => task);
            return result;
        }
        
        public static Func<(RoleManageModel roleRule, ReactionRoleConfig config),
                (RoleManageDomain roleRule, ReactionRoleConfig config)>
            MapReactionRoleAction(IGuild guild, IGuildUser user)
        {
            return x =>
            {
                var (roleRule, config) = x;
                return (RoleUtils.MapRoles(guild, user)(roleRule), config);
            };
        }
        
        public static Func<(RoleManageDomain roleRule, ReactionRoleConfig config), Task<(RoleUpdateEvent update, ReactionRoleConfig config)>> 
            RRManageRolesOnUser(IGuildUser user)
        {
            return async x => (await user.EditRoles(x.roleRule), x.config);
        }

        public static string GetRoleMentions(this IEnumerable<IRole> roles)
        {
            var enumerable = roles.ToList();
            if (!enumerable.Any()) return "-";
            return enumerable.Select(x => MentionUtils.MentionRole(x.Id)).Aggregate((acc, elem) => $"{acc}, {elem}");
        }
        
        public static string GetRoleMentions(this IEnumerable<ulong> roles)
        {
            var enumerable = roles.ToList();
            if (!enumerable.Any()) return "-";
            return enumerable.Select(MentionUtils.MentionRole).Aggregate((acc, elem) => $"{acc}, {elem}");
        }

        public static Embed CreateReactionRoleEmbedLog(RoleUpdateEvent update, ReactionRoleConfig config)
        {
            return new EmbedBuilder()
                        .WithAuthor(update.User)
                        .WithTitle($"Reaction Role: {config.Name}")
                        .WithDescription($"User: {MentionUtils.MentionUser(update.User.Id)}({update.User.Nickname})")
                        .AddField("Roles Added", update.RolesChanged.ToAdd.GetRoleMentions())
                        .AddField("Roles Removed", update.RolesChanged.ToRemove.GetRoleMentions())
                        .WithCurrentTimestamp()
                        .WithColor(116, 223, 207)
                        .Build();
        }
    }
}
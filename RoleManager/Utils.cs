using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RoleManager.Model;
using RoleManager.Service;

namespace RoleManager
{
    public static class RoleUtils
    {
        public static Func<RoleManageModel, RoleManageDomain> 
            MapRoles(IGuild guild, IGuildUser user)
        {
            return x => new RoleManageDomain(x.ToAdd.MapToGuildRoles(guild), user.GetMatchingRoles(x.ToRemove).MapToGuildRoles(guild));
        }

        public static ImmutableList<ulong> GetMatchingRoles(this IGuildUser user,
            IEnumerable<ulong> roles)
        {
            return user.RoleIds.Where(x => roles.Contains(x)).ToImmutableList();
        }

        public static ImmutableList<IRole> MapToGuildRoles(this IEnumerable<ulong> roles,
            IGuild guild)
        {
            return roles.Select(guild.GetRole).ToImmutableList();
        }

        public static async Task<RoleUpdateEvent> EditRoles(this IGuildUser user, RoleManageDomain rolesToChange)
        {
            user.RemoveRolesAsync(rolesToChange.ToRemove);
            user.AddRolesAsync(rolesToChange.ToAdd);
            return new RoleUpdateEvent(user, rolesToChange);
        }

        public static string Stringify(this IEnumerable<IRole> roles)
        {
            var socketRoles = roles.ToList();
            if (!socketRoles.Any()) return "";
            return socketRoles.Select(x => x.Name).Aggregate((acc, role) => acc + ", " + role);
        }
    }

    public static class ReactionRoleUtils
    {
        public static Func<GuildReactionRole, Result<ChannelReactionRole>> TryGetChannel(ulong channelId)
        {
            return guild => guild.TryGetChannel(channelId);
        }

        public static Func<ChannelReactionRole, Result<IReactionRoleModel>> TryGetMessage(ulong messageId)
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

    public static class ResultUtils
    {
        public static Result<TOut> BranchingIf<TIn, TOut>(this Result<TIn> result, Func<TIn, bool> predicate, Func<TIn, Result<TOut>> Then,
            Func<TIn, Result<TOut>> Else)
        {
            return result.Match(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => new Failure<TOut>(x));
        }
        
        public static Task<Result<TOut>> BranchingIf<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, bool> predicate, Func<TIn, Result<TOut>> Then,
            Func<TIn, Result<TOut>> Else)
        {
            return result.Match(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => new Failure<TOut>(x));
        }
        
        public static Task<Result<TOut>> BranchingIfAwait<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, bool> predicate, Func<TIn, Task<Result<TOut>>> Then,
            Func<TIn, Task<Result<TOut>>> Else)
        {
            return result.MatchAwait(
                Success: x => predicate(x) ? Then(x) : Else(x),
                Failure: x => Task.FromResult((Result<TOut>)x));
        }
    }
    public static class CommandExtensions{
        public static GuildConfigModel GetModelOrDefault(this Result<GuildConfigModel> result, IGuild guild)
        {
            return result.SuccessOrDefault(new GuildConfigModel(guild.Id, 0, ImmutableHashSet<ulong>.Empty));
        }
        public static bool IsStaff(this IGuildUser user, ImmutableHashSet<ulong>staffRoles)
        {
            return user.GuildPermissions.Administrator || user.RoleIds.Any(staffRoles.Contains);
        }
        
        public static Embed CreateConfigInfoEmbedLog(GuildConfigModel config)
        {
            return new EmbedBuilder()
                .WithTitle($"Configuration Info")
                .AddField("Logging Channel", config.LogChannel==0?"-":MentionUtils.MentionChannel(config.LogChannel))
                .AddField("Staff Roles", config.StaffRoles.GetRoleMentions())
                .WithCurrentTimestamp()
                .WithColor(116, 223, 207)
                .Build();
        }
        
        public static ReactionRoleModel UpdateRRModel(this ReactionRoleModel model, IEmote emote, RoleManageModel rule)
        {
            if (model.Rule is not ReactionRuleModel rrModel) return model;
            return model with {Rule = rrModel.WithNewRule(emote.Name, rule)};
        }

        public static Embed CreateReactionRoleMessageEmbed(string title, string embed)
        {
            return new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(embed)
                .WithColor(116, 223, 207)
                .WithCurrentTimestamp()
                .Build();
        }
        
        public static Embed CreateReactionRoleRuleEmbed(string emote, List<ulong> rolesToAdd, List<ulong> rolesToRemove)
        {
            return new EmbedBuilder()
                .WithTitle("Reaction Role Rule")
                .AddField("Reaction", emote)
                .AddField("Roles To Add", rolesToAdd.GetRoleMentions())
                .AddField("Roles To Remove", rolesToRemove.GetRoleMentions())
                .WithColor(116, 223, 207)
                .WithCurrentTimestamp()
                .Build();
        }

        public static ReactionRoleModel WithEmoteLinked(this ReactionRoleModel model, string emote)
        {
            if (model.Rule is not ReverseRuleModel ruleModel) return model;
            return model with {Rule = ruleModel.WithEmote(emote)};
        }
    }
}
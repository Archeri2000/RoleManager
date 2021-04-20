using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharp_Result;
using Discord;
using RoleManager.Model;

namespace RoleManager.Utils
{
    public static class CommandUtils{
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
        
        public static Embed CreateReactionRoleRuleEmbed(ReactionRoleModel model)
        {
            var builder = new EmbedBuilder()
                .WithTitle($"Reaction Role: {model.Name}")
                .AddField("Channel", MentionUtils.MentionChannel(model.ChannelId))
                .WithColor(116, 223, 207)
                .WithCurrentTimestamp();
            if (model.Rule is ReactionRuleModel rule)
            {
                foreach (var (emote, roles) in rule.Reactions)
                {
                    builder.AddRule(emote, roles);
                }
            }
            else
            {
                builder.AddField("Reaction", (model.Rule as ReverseRuleModel).Emote);
            }

            return builder.Build();
        }
        
        public static EmbedBuilder AddRule(this EmbedBuilder builder, string emote, RoleManageModel model)
        {
            return builder
                .AddField("Reaction", emote)
                .AddField("Roles To Add", model.ToAdd.GetRoleMentions())
                .AddField("Roles To Remove", model.ToRemove.GetRoleMentions());
        }
        
        public static Embed CreateJailCommandEmbed(this JailConfigModel model)
        {
            return new EmbedBuilder()
                .WithTitle("Jail Command Config")
                .AddField("Is Logged", model.ShouldLog?"True":"False")
                .AddField("Roles To Add", model.Roles.ToAdd.GetRoleMentions())
                .AddField("Roles To Remove", model.Roles.ToRemove.GetRoleMentions())
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
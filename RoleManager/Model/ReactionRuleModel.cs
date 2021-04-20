using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharp_Result;

namespace RoleManager.Model
{
    public record ReactionRuleModel:IReactionRuleModel
    {
        public ImmutableDictionary<string, RoleManageModel> Reactions
        {
            get;
            init;
        } = new Dictionary<string,RoleManageModel>().ToImmutableDictionary();

        public Result<(RoleManageModel roles, ReactionRoleConfig config)> TryGetAction(string emote)
        {
            if (Reactions.TryGetValue(emote, out var action))
            {
                return (action, Config);
            }

            return new KeyNotFoundException("Emote does not have a rule attached!");
        }

        public ReactionRuleModel WithNewRule(string emote, RoleManageModel roles)
        {
            var newRule =
                new KeyValuePair<string, RoleManageModel>(emote, roles);
            return this with {Reactions = Reactions.Append(newRule).ToImmutableDictionary()};
        }

        public ReactionRoleConfig Config { get; init; }
    }

    public interface IReactionRuleModel
    {
        public ReactionRoleConfig Config { get; init; }
    }

    public record ReverseRuleModel : IReactionRuleModel
    {
        public ReactionRoleConfig Config { get; init; }
        public string Emote { get; init; }

        public ReverseRuleModel WithEmote(string emote)
        {
            return this with {Emote = emote};
        }
    }
    
    public record ReactionRoleConfig(bool ShouldRemoveReaction, bool ShouldLogResult, bool ShouldStoreData, Guid StorageKey, string Name);

}
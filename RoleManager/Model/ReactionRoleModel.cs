using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleManager.Model
{
    public record ReactionRoleModel
    {
        public ReactionRoleModel(string name, ulong guildId, ulong channelId, ulong messageId, ReactionRuleModelBase rule)
        {
            Name = name;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Rule = rule;
            RuleId = rule.Id;
        }
        
        public ReactionRoleModel(){}

        public string Name { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public ReactionRuleModelBase Rule { get; init; }
        
        public Guid RuleId { get; init; }
        
        public ReactionRoleStorageModel ToStorage()
        {
            return new ReactionRoleStorageModel()
            {
                Name = Name,
                GuildId = GuildId.MapUlongToLong(),
                ChannelId = ChannelId.MapUlongToLong(),
                MessageId = MessageId.MapUlongToLong(),
                Rule = Rule,
                RuleId = RuleId
            };
        }
    }
    
    public record ReactionRoleStorageModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; init; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildId { get; init; }
        public long ChannelId { get; init; }
        public long MessageId { get; init; }
        public ReactionRuleModelBase Rule { get; init; }
        
        public Guid RuleId { get; init; }
        

        public ReactionRoleModel ToDomain()
        {
            return new ReactionRoleModel
            {
                Name = Name,
                GuildId = GuildId.MapLongToUlong(),
                ChannelId = ChannelId.MapLongToUlong(),
                MessageId = MessageId.MapLongToUlong(),
                Rule = Rule,
                RuleId = RuleId
            };
        }
    }
}
namespace RoleManager.Model
{
    public record ReactionRoleModel
    {
        public string Name { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public ReactionRuleModelBase Rule { get; init; }
        
        public ReactionRoleStorageModel ToStorage()
        {
            return new ReactionRoleStorageModel()
            {
                Name = Name,
                GuildId = GuildId.MapUlongToLong(),
                ChannelId = ChannelId.MapUlongToLong(),
                MessageId = MessageId.MapUlongToLong(),
                Rule = Rule
            };
        }
    }
    
    public record ReactionRoleStorageModel
    {
        public string Name { get; init; }
        public long GuildId { get; init; }
        public long ChannelId { get; init; }
        public long MessageId { get; init; }
        public ReactionRuleModelBase Rule { get; init; }

        public ReactionRoleModel ToDomain()
        {
            return new ReactionRoleModel
            {
                Name = Name,
                GuildId = GuildId.MapLongToUlong(),
                ChannelId = ChannelId.MapLongToUlong(),
                MessageId = MessageId.MapLongToUlong(),
                Rule = Rule
            };
        }
    }
}
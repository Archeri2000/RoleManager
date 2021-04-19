namespace RoleManager.Model
{
    public record ReactionRoleModel
    {
        public string Name { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public IReactionRoleModel Rule { get; init; }
        
    }
}
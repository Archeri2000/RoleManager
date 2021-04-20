namespace RoleManager.Model
{
    public record JailConfigModel(ulong guildId, bool ShouldLog, RoleManageModel Roles);
}
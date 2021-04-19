using System.Collections.Immutable;

namespace RoleManager.Model
{
    public record GuildConfigModel(ulong GuildId, ulong LogChannel, ImmutableHashSet<ulong> StaffRoles)
    {
        public GuildConfigModel WithAddedStaff(ulong staffRole)
        {
            return this with {StaffRoles = StaffRoles.Add(staffRole)};
        }
        public GuildConfigModel WithRemovedStaff(ulong staffRole)
        {
            return this with {StaffRoles = StaffRoles.Remove(staffRole)};
        }
        
    }
}
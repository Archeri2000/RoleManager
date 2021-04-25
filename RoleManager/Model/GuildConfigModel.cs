using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoleManager.Model
{
    public record GuildConfigModel
    {
        public ulong GuildId { get; init; }
        public ulong LogChannel { get; init; }
        public List<ulong> Roles { get; init; }
        public GuildConfigModel(ulong guildId, ulong logChannel, IEnumerable<ulong> roles)
        {
            GuildId = guildId;
            LogChannel = logChannel;
            Roles = roles.ToList();
        }
        
        public GuildConfigModel(){}
        public ImmutableHashSet<ulong> StaffRoles => Roles.ToImmutableHashSet();

        public GuildConfigModel WithAddedStaff(ulong staffRole)
        {
            return this with {Roles = StaffRoles.Add(staffRole).ToList()};
        }
        public GuildConfigModel WithRemovedStaff(ulong staffRole)
        {
            return this with {Roles = StaffRoles.Remove(staffRole).ToList()};
        }
        
    }
    
}
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

        public GuildConfigStorageModel ToStorage()
        {
            return new GuildConfigStorageModel(GuildId.MapUlongToLong(), LogChannel.MapUlongToLong(),
                Roles.Select(LongConverters.MapUlongToLong).ToList());
        }
    }
    
    public record GuildConfigStorageModel
    {
        public long GuildId { get; init; }
        public long LogChannel { get; init; }
        public List<long> Roles { get; init; }
        public GuildConfigStorageModel(long guildId, long logChannel, IEnumerable<long> roles)
        {
            GuildId = guildId;
            LogChannel = logChannel;
            Roles = roles.ToList();
        }
        
        public GuildConfigStorageModel(){}

        public GuildConfigModel ToDomain()
        {
            return new GuildConfigModel(GuildId.MapLongToUlong(), LogChannel.MapLongToUlong(),
                Roles.Select(LongConverters.MapLongToUlong));
        }

    }

    public static class LongConverters
    {
        public static long MapUlongToLong(this ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }
        
        public static ulong MapLongToUlong(this long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }
    }
    
}
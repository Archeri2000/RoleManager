using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleManager.Model
{
    public record JailConfigModel
    {
        public JailConfigModel(ulong guildId, bool shouldLog, ulong logChannel, RoleManageModel roles)
        {
            GuildId = guildId;
            ShouldLog = shouldLog;
            LogChannel = logChannel;
            Roles = roles;
        }
        
        public JailConfigModel(){}
        public ulong GuildId { get; init; }
        public bool ShouldLog { get; init; }
        public ulong LogChannel { get; init; }
        public RoleManageModel Roles { get; init; }
        
        public JailConfigStorageModel ToStorage()
        {
            return new JailConfigStorageModel(GuildId.MapUlongToLong(), ShouldLog, LogChannel.MapUlongToLong(),
                Roles.ToStorage());
        }
    }
    
    public record JailConfigStorageModel
    {
        public JailConfigStorageModel(long guildId, bool shouldLog, long logChannel, RoleManageStorageModel roles)
        {
            GuildId = guildId;
            ShouldLog = shouldLog;
            LogChannel = logChannel;
            Add = roles.Add;
            Remove = roles.Remove;
        }
        
        public JailConfigStorageModel(){}
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildId { get; init; }
        public bool ShouldLog { get; init; }
        public long LogChannel { get; init; }
        
        public List<long> Add { get; init; }
        
        public List<long> Remove { get; init; }

        public JailConfigModel ToDomain()
        {
            return new JailConfigModel(GuildId.MapLongToUlong(), ShouldLog, LogChannel.MapLongToUlong(),
                new RoleManageModel(Add, Remove));
        }
    }

    public record JailTimeSpan(int Days, int Hours, int Minutes)
    {
        public TimeSpan ToTimeSpan()
        {
            return new(Days, Hours, Minutes, 0);
        }

        public override string ToString()
        {
            return $"{Days} Days, {Hours} Hours, {Minutes} Minutes";
        }

        public bool IsZero()
        {
            return Days == 0 && Hours == 0 && Minutes == 0;
        }
    }

    public record JailDataStorage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildId { get; init; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; init; }

        public JailDataStorage(long guildId, long userId, RoleManageStorageModel roles)
        {
            UserId = userId;
            GuildId = guildId;
            Add = roles.Add;
            Remove = roles.Remove;
        }
        
        public List<long> Add { get; init; }
        
        public List<long> Remove { get; init; }
        
        public JailDataStorage(){}
    }
}
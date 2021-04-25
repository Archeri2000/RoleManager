using System;

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

    public record JailData
    {
        public ulong GuildId { get; init; }
        public RoleUpdateModel Roles { get; init; }
        
        public ulong UserId { get; init; }

        public JailData(ulong guildId, RoleUpdateModel roles)
        {
            UserId = Roles.User;
            GuildId = guildId;
            Roles = roles;
        }
        
        public JailData(){}
    }
}
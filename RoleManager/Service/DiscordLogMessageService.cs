using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Discord;
using RoleManager.Repository;

namespace RoleManager.Service
{
    public class DiscordLogMessageService: IDiscordLogMessageService
    {
        private IGuildConfigRepository _config;
        private ILoggingService _logging;

        public DiscordLogMessageService(IGuildConfigRepository config, ILoggingService logging)
        {
            _config = config;
            _logging = logging;
        }
        public async Task<Result<Unit>> WriteLogToGuild(IGuild guild, Embed embed)
        {
            var res = await _config.GetGuildConfig(guild.Id);
            if (res.IsFailure())
            {
                return new Unit();
            }
            var config = res.Get();
            if (await guild.GetChannelAsync(config.LogChannel) is not ITextChannel channel)
            {
                return new Unit();
            }
            await channel.SendMessageAsync(embed:embed);
            return new Unit();
        }

        public async Task<Result<Unit>> WriteLogToGuild(IGuild guild, string message)
        {
            var res = await _config.GetGuildConfig(guild.Id);
            if (res.IsFailure())
            {
                return new Unit();
            }
            var config = res.Get();
            if (await guild.GetChannelAsync(config.LogChannel) is not ITextChannel channel)
            {
                return new Unit();
            }
            await channel.SendMessageAsync(message);
            return new Unit();        }

        public async Task<Result<Unit>> TryAlertStaff(IGuild guild, string message)
        {
            var res = await _config.GetGuildConfig(guild.Id);
            if (res.IsFailure())
            {
                return new Unit();
            }
            var config = res.Get();
            if (config.StaffRoles.IsEmpty)
            {
                return new Unit();
            }
            var roles = config.StaffRoles.Select(MentionUtils.MentionRole).Aggregate((acc, elem) => $"{acc}, {elem}");
            return await WriteLogToGuild(guild, $"**Pinging {roles}...**\n{message}");
        }
    }
}
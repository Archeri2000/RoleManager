using System.Threading.Tasks;
using CSharp_Result;
using Discord;

namespace RoleManager.Service
{
    public interface IDiscordLogMessageService
    {
        public Task<Result<Unit>> WriteLogToGuild(IGuild guild, Embed embed);
        public Task<Result<Unit>> WriteLogToGuild(IGuild guild, string message);

        public Task<Result<Unit>> TryAlertStaff(IGuild guild, string message);
    }
}
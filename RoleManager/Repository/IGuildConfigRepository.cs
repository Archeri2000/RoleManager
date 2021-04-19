using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IGuildConfigRepository
    {
        public Task<Result<GuildConfigModel>> GetGuildConfig(ulong guildId);
        public Task<Result<ulong>> StoreGuildConfig(GuildConfigModel config);

    }
}
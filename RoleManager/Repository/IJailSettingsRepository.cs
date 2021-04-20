using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IJailSettingsRepository
    {
        public Task<Result<JailConfigModel>> GetJailConfig(ulong guildId);
        public Task<Result<ulong>> StoreJailConfig(JailConfigModel config);

    }
}
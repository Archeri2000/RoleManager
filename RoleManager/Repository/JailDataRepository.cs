using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class JailDataRepository:IJailDataRepository
    {
        public async Task<Result<Unit>> Store(ulong guildId, RoleUpdateModel updateModel)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result<Unit>> Delete(ulong guildId, ulong userId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result<RoleUpdateModel>> Load(ulong guildId, ulong userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
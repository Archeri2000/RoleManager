using System;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IJailDataRepository
    {
        Task<Result<Unit>> Store(ulong guildId, RoleUpdateModel updateModel);
        Task<Result<Unit>> Delete(ulong guildId, ulong userId);
        Task<Result<RoleUpdateModel>> Load(ulong guildId, ulong userId);
    }
}
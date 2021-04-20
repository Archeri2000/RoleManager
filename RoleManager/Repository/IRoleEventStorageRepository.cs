using System;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IRoleEventStorageRepository
    {
        Task<Result<Unit>> Store(Guid storageKey, RoleUpdateModel updateModel);
        Task<Result<RoleUpdateModel>> Load(Guid storageKey, ulong userId);
    }
}
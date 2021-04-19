using System;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IRoleEventStorageRepository
    {
        Task<Result<Unit>> Store(Guid storageKey, RoleUpdateEvent updateEvent);
        Task<Result<RoleUpdateEvent>> Load(Guid storageKey, ulong userId);
    }
}
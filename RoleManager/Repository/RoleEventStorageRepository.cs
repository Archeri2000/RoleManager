using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class RoleEventStorageRepository : IRoleEventStorageRepository
    {
        private Dictionary<Guid, Dictionary<ulong, RoleUpdateModel>> _storage = new();
        public async Task<Result<Unit>> Store(Guid storageKey, RoleUpdateModel updateModel)
        {
            _storage.TryAdd(storageKey, new Dictionary<ulong, RoleUpdateModel>());
            _storage[storageKey][updateModel.User] = updateModel;
            return new Unit();
        }

        public async Task<Result<RoleUpdateModel>> Load(Guid storageKey, ulong userId)
        {
            RoleUpdateModel updateModel = null;
            if (!_storage.TryGetValue(storageKey, out var dict) || !dict.TryGetValue(userId, out updateModel))
            {
                return new KeyNotFoundException("Model not found in repository!");
            }
            return updateModel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class RoleEventStorageRepository : IRoleEventStorageRepository
    {
        private Dictionary<Guid, Dictionary<ulong, RoleUpdateEvent>> _storage = new();
        public async Task<Result<Unit>> Store(Guid storageKey, RoleUpdateEvent updateEvent)
        {
            _storage.TryAdd(storageKey, new Dictionary<ulong, RoleUpdateEvent>());
            _storage[storageKey][updateEvent.User.Id] = updateEvent;
            return new Unit();
        }

        public async Task<Result<RoleUpdateEvent>> Load(Guid storageKey, ulong userId)
        {
            RoleUpdateEvent updateEvent = null;
            if (!_storage.TryGetValue(storageKey, out var dict) || !dict.TryGetValue(userId, out updateEvent))
            {
                return new KeyNotFoundException("Model not found in repository!");
            }
            return updateEvent;
        }
    }
}
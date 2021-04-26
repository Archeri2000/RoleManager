using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Microsoft.EntityFrameworkCore;
using RoleManager.Database;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class RoleEventStorageRepository : IRoleEventStorageRepository
    {
        private readonly CoreDbContext _context;

        public RoleEventStorageRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RoleUpdateModel>> Load(Guid storageKey, ulong userId)
        {
            var uid = userId.MapUlongToLong();
            var result = await AsyncEnumerable.FirstOrDefaultAsync(_context.Events, x => x.User == uid && x.StorageKey == storageKey);
            if (result == null)
            {
                return new KeyNotFoundException();
            }
            return result.ToModel();
        }
        
        
        public async Task<Result<Unit>> Store(Guid storageKey, RoleUpdateModel updateModel)
        {
            var storageEvent = updateModel.ToStorage(storageKey);
            var entity =
                await (_context.Events as IQueryable<RoleEventStorageModel>)
                    .FirstOrDefaultAsync(x =>
                        x.StorageKey == storageEvent.StorageKey && x.User == storageEvent.User);
            if (entity is null)
            {
                _context.Events.Add(storageEvent);
            }
            else
            {
                _context.Entry(entity).CurrentValues.SetValues(storageEvent);
            }

            await _context.SaveChangesAsync();
            return new Unit();
        }
    }
    
    public class MockRoleEventStorageRepository : IRoleEventStorageRepository
    {
        private readonly Dictionary<Guid, Dictionary<ulong, RoleUpdateModel>> _storage = new();
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
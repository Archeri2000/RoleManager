using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Microsoft.EntityFrameworkCore;
using RoleManager.Database;
using RoleManager.Model;
using Z.EntityFramework.Plus;

namespace RoleManager.Repository
{
    public class JailDataRepository:IJailDataRepository
    {
        private readonly CoreDbContext _context;

        public JailDataRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Store(ulong guildId, RoleUpdateModel updateModel)
        {
            var storage = new JailDataStorage(guildId.MapUlongToLong(), updateModel.User.MapUlongToLong(),
                updateModel.RolesChanged.ToStorage());
            var entity =
                await (_context.JailDatas as IQueryable<JailDataStorage>)
                    .FirstOrDefaultAsync(x =>
                        x.GuildId == storage.GuildId && x.UserId == storage.UserId);
            if (entity is null)
            {
                _context.JailDatas.Add(storage);
            }
            else
            {
                _context.Entry(entity).CurrentValues.SetValues(storage);
            }

            await _context.SaveChangesAsync();
            return new Unit();
        }

        public async Task<Result<Unit>> Delete(ulong guildId, ulong userId)
        {
            var gid = guildId.MapUlongToLong();
            var uid = userId.MapUlongToLong();
            var result = await (_context.JailDatas as IQueryable<JailDataStorage>).FirstOrDefaultAsync(x => x.GuildId == gid && x.UserId == uid);
            if (result != null)
            {
                _context.Remove(result);
                await _context.SaveChangesAsync();
            }
            return new Unit();
        }

        public async Task<Result<RoleUpdateModel>> Load(ulong guildId, ulong userId)
        {
            var gid = guildId.MapUlongToLong();
            var uid = userId.MapUlongToLong();
            var result = await AsyncEnumerable.FirstOrDefaultAsync(_context.JailDatas, x => x.GuildId == gid && x.UserId == uid);
            if (result == null)
            {
                return new KeyNotFoundException();
            }
            return new RoleUpdateModel(result.UserId.MapLongToUlong(), new RoleManageModel(result.Add, result.Remove));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
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
            var storageData = new JailDataStorage(guildId.MapUlongToLong(), updateModel.User.MapUlongToLong(),
                updateModel.RolesChanged.ToStorage());
            _context.JailDatas.Update(storageData);
            await _context.SaveChangesAsync();
            return new Unit();
        }

        public async Task<Result<Unit>> Delete(ulong guildId, ulong userId)
        {
            var gid = guildId.MapUlongToLong();
            var uid = userId.MapUlongToLong();
            var result = await (_context.JailDatas as IQueryable<JailDataStorage>).Where(x => x.GuildId == gid && x.UserId == uid).DeleteAsync();
            return new Unit();
        }

        public async Task<Result<RoleUpdateModel>> Load(ulong guildId, ulong userId)
        {
            var gid = guildId.MapUlongToLong();
            var uid = userId.MapUlongToLong();
            var result = await _context.JailDatas.FirstOrDefaultAsync(x => x.GuildId == gid && x.UserId == uid);
            if (result == null)
            {
                return new KeyNotFoundException();
            }
            return new RoleUpdateModel(result.UserId.MapLongToUlong(), result.Roles.ToDomain());
        }
    }
}
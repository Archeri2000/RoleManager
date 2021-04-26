using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Microsoft.EntityFrameworkCore;
using RoleManager.Database;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class JailSettingsRepository:IJailSettingsRepository
    {
        private readonly CoreDbContext _context;

        public JailSettingsRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<JailConfigModel>> GetJailConfig(ulong guildId)
        {
            var id = guildId.MapUlongToLong();
            var result = await (_context.JailConfigModels as IQueryable<JailConfigStorageModel>).FirstOrDefaultAsync(x => x.GuildId == id);
            return result == null ? new KeyNotFoundException() : result.ToDomain();
        }

        public async Task<Result<ulong>> StoreJailConfig(JailConfigModel config)
        {
            var storage = config.ToStorage();
            var entity =
                (_context.JailConfigModels as IQueryable<JailConfigStorageModel>)
                    .FirstOrDefaultAsync(x =>
                        x.GuildId == storage.GuildId);
            if (entity is null)
            {
                _context.JailConfigModels.Add(storage);
            }
            else
            {
                _context.Entry(entity).CurrentValues.SetValues(storage);
            }

            await _context.SaveChangesAsync();
            return config.GuildId;
        }
    }
}
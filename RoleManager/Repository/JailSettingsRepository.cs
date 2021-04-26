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
            await _context.JailConfigModels.Upsert(config.ToStorage()).RunAsync();
            return config.GuildId;
        }
    }
}
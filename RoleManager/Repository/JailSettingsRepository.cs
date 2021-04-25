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
            var result = await (_context.JailConfigModels as IQueryable<JailConfigModel>).FirstOrDefaultAsync(x => x.GuildId == guildId);
            return result == null ? new KeyNotFoundException() : result;
        }

        public async Task<Result<ulong>> StoreJailConfig(JailConfigModel config)
        {
            var model = _context.Update(config);
            await _context.SaveChangesAsync();
            return model.Entity.GuildId;
        }
    }
}
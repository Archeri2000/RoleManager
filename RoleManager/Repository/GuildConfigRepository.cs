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
    public class GuildConfigRepository:IGuildConfigRepository
    {
        private readonly CoreDbContext _context;

        public GuildConfigRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<GuildConfigModel>> GetGuildConfig(ulong guildId)
        {
            var result = await (_context.GuildConfigModels as IQueryable<GuildConfigModel>).FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (result == null)
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return result;
            //return new GuildConfigModel(826478889181511731, 831565191795834880, new List<ulong>{826480593315168266}.ToImmutableHashSet());
        }

        public async Task<Result<ulong>> StoreGuildConfig(GuildConfigModel conf)
        {
            try
            {
                _context.GuildConfigModels.Update(conf);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
                throw e;
            }

            return conf.GuildId;
        }
    }
    
    public class MockGuildConfigRepository:IGuildConfigRepository
    {
        private readonly Dictionary<ulong, GuildConfigModel> _configs = new();
        public async Task<Result<GuildConfigModel>> GetGuildConfig(ulong guildId)
        {
            if (!_configs.TryGetValue(guildId, out var conf))
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return conf;
            //return new GuildConfigModel(826478889181511731, 831565191795834880, new List<ulong>{826480593315168266}.ToImmutableHashSet());
        }

        public async Task<Result<ulong>> StoreGuildConfig(GuildConfigModel conf)
        {
            _configs[conf.GuildId] = conf;
            return conf.GuildId;
        }
    }
}
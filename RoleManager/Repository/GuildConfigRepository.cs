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
            var id = guildId.MapUlongToLong();
            var result = await (_context.GuildConfigModels as IQueryable<GuildConfigStorageModel>).FirstOrDefaultAsync(x => x.GuildId == id);
            if (result == null)
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return result.ToDomain();
            //return new GuildConfigModel(826478889181511731, 831565191795834880, new List<ulong>{826480593315168266}.ToImmutableHashSet());
        }

        public async Task<Result<ulong>> StoreGuildConfig(GuildConfigModel conf)
        {
            var storage = conf.ToStorage();
            var entity =
                await (_context.GuildConfigModels as IQueryable<GuildConfigStorageModel>)
                    .FirstOrDefaultAsync(x =>
                        x.GuildId == storage.GuildId);
            if (entity is null)
            {
                _context.GuildConfigModels.Add(storage);
            }
            else
            {
                _context.Entry(entity).CurrentValues.SetValues(storage);
            }

            await _context.SaveChangesAsync();
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
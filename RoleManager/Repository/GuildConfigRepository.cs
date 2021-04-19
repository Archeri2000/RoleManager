using System.Collections.Generic;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class GuildConfigRepository:IGuildConfigRepository
    {
        private Dictionary<ulong, GuildConfigModel> configs = new();
        public async Task<Result<GuildConfigModel>> GetGuildConfig(ulong guildId)
        {
            if (!configs.TryGetValue(guildId, out var conf))
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return conf;
            //return new GuildConfigModel(826478889181511731, 831565191795834880, new List<ulong>{826480593315168266}.ToImmutableHashSet());
        }

        public async Task<Result<ulong>> StoreGuildConfig(GuildConfigModel conf)
        {
            configs[conf.GuildId] = conf;
            return conf.GuildId;
        }
    }
}
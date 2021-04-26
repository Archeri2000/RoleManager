using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharp_Result;
using Microsoft.EntityFrameworkCore;
using RoleManager.Database;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public class ReactionRoleRuleRepository : IReactionRoleRuleRepository
    {
        private readonly CoreDbContext _context;

        public ReactionRoleRuleRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<ReactionRoleModel>>> GetReactionRoles()
        {
            return (await EntityFrameworkQueryableExtensions.ToListAsync(_context.ReactionRoleModels)).Select(x => x.ToDomain()).ToResult();
        }

        public async Task<Result<ReactionRoleModel>> GetReactionRole(ulong guild, string name)
        {
            var id = guild.MapUlongToLong();
            var result = await (_context.ReactionRoleModels as IQueryable<ReactionRoleStorageModel>).FirstOrDefaultAsync(x => x.GuildId == id && x.Name == name);
            if (result == null)
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return result.ToDomain();
        }

        public async Task<bool> AddOrUpdateReactionRole(ReactionRoleModel reactionRole)
        {
            _context.ReactionRoleModels.Update(reactionRole.ToStorage());
            await _context.SaveChangesAsync();
            return true;
        }
    }
    
    public class MockReactionRoleRuleRepository : IReactionRoleRuleRepository
    {
        private Dictionary<(ulong guildId, string name), ReactionRoleModel> _models = new();
        //TODO
        public async Task<Result<IEnumerable<ReactionRoleModel>>> GetReactionRoles()
        {
            return _models.Values;
        }

        public async Task<Result<ReactionRoleModel>> GetReactionRole(ulong guild, string name)
        {
            if (_models.TryGetValue((guild, name), out var ret))
            {
                return ret;
            }

            return new KeyNotFoundException();
        }

        public async Task<bool> AddOrUpdateReactionRole(ReactionRoleModel reactionRole)
        {
            _models[(reactionRole.GuildId, reactionRole.Name)] = reactionRole;
            return true;
        }
    }
}
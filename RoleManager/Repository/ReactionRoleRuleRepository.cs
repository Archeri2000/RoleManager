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
    public class ReactionRoleRuleRepository : IReactionRoleRuleRepository
    {
        private readonly CoreDbContext _context;

        public ReactionRoleRuleRepository(CoreDbContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<ReactionRoleModel>>> GetReactionRoles()
        {
            return (await _context.ReactionRoleModels.Include(x => x.Rule).ToListAsync()).Select(x => x.ToDomain()).ToResult();
        }

        public async Task<Result<ReactionRoleModel>> GetReactionRole(ulong guild, string name)
        {
            Console.WriteLine($"GetRR Called with {guild} and {name}");
            var id = guild.MapUlongToLong();
            ReactionRoleStorageModel result;
            try
            {
                result =
                    await _context.ReactionRoleModels
                        .Include(x => x.Rule)
                        .FirstOrDefaultAsync(x =>
                        x.GuildId == id && x.Name == name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.InnerException);
                throw;
            }
            
            Console.WriteLine($"Result is: {(result is null?"null":"not null")}");
            if (result is null)
            {
                return new KeyNotFoundException("Unable to find guild config!");
            }

            return result.ToDomain();
        }

        public async Task<bool> AddOrUpdateReactionRole(ReactionRoleModel reactionRole)
        {
            try
            {
                var id = reactionRole.Rule switch
                {
                    ReactionRuleModel model => _context.ReactionRuleModels.Update(model).Entity.Id,
                    ReverseRuleModel model => _context.ReverseRuleModels.Update(model).Entity.Id
                };
                await _context.SaveChangesAsync();
                var storage = reactionRole.ToStorage() with {RuleId = id};
                var entity =
                    _context.ReactionRoleModels.Include(x => x.Rule)
                        .FirstOrDefaultAsync(x =>
                        x.Name == storage.Name && x.GuildId == storage.GuildId);
                if (entity is null)
                {
                    _context.ReactionRoleModels.Add(storage);
                }
                else
                {
                    _context.Entry(entity).CurrentValues.SetValues(storage);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

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
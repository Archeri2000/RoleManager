using System.Collections.Generic;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;

namespace RoleManager.Repository
{
    public interface IReactionRoleRuleRepository
    {
        public Task<Result<IEnumerable<ReactionRoleModel>>> GetReactionRoles();

        public Task<Result<ReactionRoleModel>> GetReactionRole(ulong guild, string name);
        public Task<bool> AddOrUpdateReactionRole(ReactionRoleModel reactionRole);

        public Task<bool> DeleteReactionRole(ulong guild, string name);
    }
}
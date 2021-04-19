using System.Collections.Generic;
using System.Threading.Tasks;
using CSharp_Result;
using RoleManager.Model;
using RoleManager.Service;

namespace RoleManager.Repository
{
    public class ReactionRoleRuleRepository : IReactionRoleRuleRepository
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
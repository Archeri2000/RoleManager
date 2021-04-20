using System;
using System.Collections.Immutable;
using System.Linq;
using Discord;

namespace RoleManager.Model
{
    public record RoleManageModel(ImmutableHashSet<ulong> ToAdd, ImmutableHashSet<ulong> ToRemove);

    public record RoleManageDomain(ImmutableList<IRole> ToAdd, ImmutableList<IRole> ToRemove)
    {
        public RoleManageModel ToModel()
        {
            return new RoleManageModel(ToAdd.Select(x => x.Id).ToImmutableHashSet(),
                ToRemove.Select(x => x.Id).ToImmutableHashSet());
        }
    }

    public record RoleUpdateModel(ulong User, RoleManageModel RolesChanged);

    public record RoleEventStorageModel(Guid StorageKey, ulong User, RoleManageModel RolesChanged);

    public record RoleUpdateEvent(IGuildUser User, RoleManageDomain RolesChanged)
    {
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User.Id, RolesChanged.ToModel());
        }
    }

}
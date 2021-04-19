using System.Collections.Immutable;
using Discord;

namespace RoleManager.Model
{
    public record RoleManageModel(ImmutableHashSet<ulong> ToAdd, ImmutableHashSet<ulong> ToRemove);
    public record RoleManageDomain(ImmutableList<IRole> ToAdd, ImmutableList<IRole> ToRemove);

    public record RoleUpdateEvent(IGuildUser User, RoleManageDomain RolesChanged);
}
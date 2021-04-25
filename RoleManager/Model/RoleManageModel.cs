using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Discord;

namespace RoleManager.Model
{
    public record RoleManageModel
    {
        public List<ulong> Add { get; init; }
        public List<ulong> Remove { get; init; }
        [NotMapped]
        public ImmutableHashSet<ulong> ToAdd => Add.ToImmutableHashSet();
        [NotMapped]
        public ImmutableHashSet<ulong> ToRemove => Remove.ToImmutableHashSet();

        public RoleManageModel(IEnumerable<ulong> add, IEnumerable<ulong> remove)
        {
            Add = add.ToList();
            Remove = remove.ToList();
        }
        
        public RoleManageModel(){}
    };

    public record RoleManageDomain(ImmutableList<IRole> ToAdd, ImmutableList<IRole> ToRemove)
    {
        public RoleManageModel ToModel()
        {
            return new RoleManageModel(ToAdd.Select(x => x.Id).ToImmutableHashSet(),
                ToRemove.Select(x => x.Id).ToImmutableHashSet());
        }
    }

    public record RoleUpdateModel
    {
        public RoleEventStorageModel ToStorage(Guid storageKey)
        {
            return new RoleEventStorageModel(storageKey, User, RolesChanged);
        }

        public RoleUpdateModel(ulong user, RoleManageModel rolesChanged)
        {
            User = user;
            RolesChanged = rolesChanged;
        }
        
        public RoleUpdateModel(){}

        public ulong User { get; init; }
        public RoleManageModel RolesChanged { get; init; }
    }

    public record RoleEventStorageModel
    {
        public RoleEventStorageModel(Guid storageKey, ulong user, RoleManageModel rolesChanged)
        {
            RolesChanged = rolesChanged;
            StorageKey = storageKey;
            User = user;
        }

        public RoleEventStorageModel()
        {
            
        }
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User, RolesChanged);
        }

        public Guid StorageKey { get; init; }
        public RoleManageModel RolesChanged { get; init; }
        public ulong User { get; init; }
    }

    public record RoleUpdateEvent(IGuildUser User, RoleManageDomain RolesChanged)
    {
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User.Id, RolesChanged.ToModel());
        }
    }

}
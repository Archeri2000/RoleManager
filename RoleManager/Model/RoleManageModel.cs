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
        
        public RoleManageStorageModel ToStorage()
        {
            return new RoleManageStorageModel(Add.Select(LongConverters.MapUlongToLong).ToList(),
                Remove.Select(LongConverters.MapUlongToLong).ToList());
        }
    }
    
    public record RoleManageStorageModel
    {
        public List<long> Add { get; init; }
        public List<long> Remove { get; init; }

        public RoleManageStorageModel(IEnumerable<long> add, IEnumerable<long> remove)
        {
            Add = add.ToList();
            Remove = remove.ToList();
        }
        
        public RoleManageStorageModel(){}

        public RoleManageModel ToDomain()
        {
            return new RoleManageModel(Add.Select(LongConverters.MapLongToUlong).ToList(),
                Remove.Select(LongConverters.MapLongToUlong).ToList());
        }
    }

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
            return new RoleEventStorageModel(storageKey, User.MapUlongToLong(), RolesChanged.ToStorage());
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
        public RoleEventStorageModel(Guid storageKey, long user, RoleManageStorageModel rolesChanged)
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
            return new RoleUpdateModel(User.MapLongToUlong(), RolesChanged.ToDomain());
        }

        public Guid StorageKey { get; init; }
        public RoleManageStorageModel RolesChanged { get; init; }
        public long User { get; init; }
    }

    public record RoleUpdateEvent(IGuildUser User, RoleManageDomain RolesChanged)
    {
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User.Id, RolesChanged.ToModel());
        }
    }

}
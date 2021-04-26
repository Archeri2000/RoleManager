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

        public RoleManageModel(IEnumerable<long> add, IEnumerable<long> remove)
        {
            Add = add.Select(LongConverters.MapLongToUlong).ToList();
            Remove = remove.Select(LongConverters.MapLongToUlong).ToList();
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
            return new RoleManageModel(Add, Remove);
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
            StorageKey = storageKey;
            User = user;
            Add = rolesChanged.Add;
            Remove = rolesChanged.Remove;
        }

        public RoleEventStorageModel()
        {
            
        }
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User.MapLongToUlong(), new RoleManageModel(Add, Remove));
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid StorageKey { get; init; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long User { get; init; }
        
        public List<long> Add { get; init; }
        
        public List<long> Remove { get; init; }
    }

    public record RoleUpdateEvent(IGuildUser User, RoleManageDomain RolesChanged)
    {
        public RoleUpdateModel ToModel()
        {
            return new RoleUpdateModel(User.Id, RolesChanged.ToModel());
        }
    }

}
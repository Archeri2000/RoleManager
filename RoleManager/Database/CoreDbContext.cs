using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using RoleManager.Model;

namespace RoleManager.Database
{
    public class CoreDbContext: DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }
        
        public DbSet<GuildConfigModel> GuildConfigModels { get; set; }
        public DbSet<ReactionRoleModel> ReactionRoleModels { get; set; }
        public DbSet<RoleEventStorageModel> Events { get; set; }
        private DbSet<IReactionRuleModel> _iReactionRuleModels { get; set; }
        public DbSet<ReactionRuleModel> ReactionRuleModels { get; set; }
        public DbSet<ReverseRuleModel> ReverseRuleModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfigModel>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<ReactionRoleModel>()
                .HasKey(x => new {x.GuildId, x.Name});

            modelBuilder.Entity<ReactionRoleModel>()
                .HasOne(x => x.Rule)
                .WithOne();
            
            modelBuilder.Entity<IReactionRuleModel>()
                .HasDiscriminator<string>("rule_type")
                .HasValue<ReactionRuleModel>("reaction_role")
                .HasValue<ReverseRuleModel>("linked_reaction_role");

            modelBuilder.Entity<IReactionRuleModel>()
                .OwnsOne(x => x.Config);
            
            modelBuilder.Entity<ReactionRuleModel>()
                .Property(b => b.Reactions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, RoleManageModel>>(v, null).ToImmutableDictionary());

            modelBuilder.Entity<RoleEventStorageModel>()
                .HasKey(x => new {x.StorageKey, x.User});

            modelBuilder.Entity<RoleEventStorageModel>()
                .OwnsOne(x => x.RolesChanged);


        }
    }
}
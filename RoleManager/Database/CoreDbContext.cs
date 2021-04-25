using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Design;
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
        private DbSet<ReactionRuleModelBase> _iReactionRuleModels { get; set; }
        public DbSet<ReactionRuleModel> ReactionRuleModels { get; set; }
        public DbSet<ReverseRuleModel> ReverseRuleModels { get; set; }
        
        public DbSet<JailConfigModel> JailConfigModels { get; set; }
        public DbSet<JailData> JailDatas { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfigModel>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<GuildConfigModel>()
                .Ignore(x => x.StaffRoles);

            modelBuilder.Entity<ReactionRoleModel>()
                .HasKey(x => new {x.GuildId, x.Name});

            modelBuilder.Entity<ReactionRoleModel>()
                .HasOne(x => x.Rule)
                .WithOne()
                .HasForeignKey("ReactionRoleModel");
            
            modelBuilder.Entity<ReactionRuleModelBase>()
                .HasDiscriminator<string>("rule_type")
                .HasValue<ReactionRuleModel>("reaction_role")
                .HasValue<ReverseRuleModel>("linked_reaction_role");

            modelBuilder.Entity<ReactionRuleModelBase>()
                .OwnsOne(x => x.Config);

            modelBuilder.Entity<ReactionRuleModelBase>().HasKey(x => x.Id);
            
            modelBuilder.Entity<ReactionRuleModel>()
                .Property(b => b.Reactions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, RoleManageModel>>(v, null).ToImmutableDictionary());

            modelBuilder.Entity<RoleEventStorageModel>()
                .HasKey(x => new {x.StorageKey, x.User});

            modelBuilder.Entity<RoleEventStorageModel>()
                .OwnsOne(x => x.RolesChanged);

            modelBuilder.Entity<JailConfigModel>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<JailConfigModel>()
                .OwnsOne(x => x.Roles);

            modelBuilder.Entity<JailData>()
                .HasKey(x => new {x.GuildId, x.UserId});

            modelBuilder.Entity<JailData>()
                .OwnsOne(x => x.Roles).OwnsOne(x => x.RolesChanged);
        }
    }
    
    public class CoreContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
    {
        public CoreDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();
            optionsBuilder.UseNpgsql("Postgres");

            return new CoreDbContext(optionsBuilder.Options);
        }
    }
}
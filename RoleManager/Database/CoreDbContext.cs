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
        
        public DbSet<GuildConfigStorageModel> GuildConfigModels { get; set; }
        public DbSet<ReactionRoleStorageModel> ReactionRoleModels { get; set; }
        public DbSet<RoleEventStorageModel> Events { get; set; }
        public DbSet<ReactionRuleModel> ReactionRuleModels { get; set; }
        public DbSet<ReverseRuleModel> ReverseRuleModels { get; set; }
        
        public DbSet<JailConfigStorageModel> JailConfigModels { get; set; }
        public DbSet<JailDataStorage> JailDatas { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfigStorageModel>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<ReactionRoleStorageModel>()
                .HasKey(x => new {x.GuildId, x.Name});

            modelBuilder.Entity<ReactionRoleStorageModel>()
                .HasOne(x => x.Rule)
                .WithOne()
                .HasForeignKey<ReactionRoleStorageModel>(x => x.RuleId);

            // modelBuilder.Entity<ReactionRuleModelBase>()
            //     .HasDiscriminator<string>("rule_type")
            //     .HasValue<ReactionRuleModel>("reaction_role")
            //     .HasValue<ReverseRuleModel>("linked_reaction_role");
            //
            // modelBuilder.Entity<ReactionRuleModelBase>()
            //     .OwnsOne(x => x.Config);
            //
            // modelBuilder.Entity<ReactionRuleModelBase>().HasKey(x => x.Id);

            modelBuilder.Entity<ReverseRuleModel>()
                .OwnsOne(x => x.Config);

            modelBuilder.Entity<ReverseRuleModel>()
                .HasBaseType<ReactionRuleModelBase>();

            modelBuilder.Entity<ReactionRuleModel>()
                .HasBaseType<ReactionRuleModelBase>();

            modelBuilder.Entity<ReactionRuleModel>()
                .OwnsOne(x => x.Config);
            
            modelBuilder.Entity<ReactionRuleModel>()
                .Property(b => b.Reactions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<Dictionary<string, RoleManageModel>>(v, null).ToImmutableDictionary());

            modelBuilder.Entity<RoleEventStorageModel>()
                .HasKey(x => new {x.StorageKey, x.User});

            modelBuilder.Entity<JailConfigStorageModel>()
                .HasKey(x => x.GuildId);

            modelBuilder.Entity<JailDataStorage>()
                .HasKey(x => new {x.GuildId, x.UserId});
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
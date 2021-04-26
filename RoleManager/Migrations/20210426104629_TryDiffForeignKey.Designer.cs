﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RoleManager.Database;

namespace RoleManager.Migrations
{
    [DbContext(typeof(CoreDbContext))]
    [Migration("20210426104629_TryDiffForeignKey")]
    partial class TryDiffForeignKey
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.5")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("RoleManager.Model.GuildConfigStorageModel", b =>
                {
                    b.Property<long>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<long>("LogChannel")
                        .HasColumnType("bigint");

                    b.Property<List<long>>("Roles")
                        .HasColumnType("bigint[]");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigModels");
                });

            modelBuilder.Entity("RoleManager.Model.JailConfigStorageModel", b =>
                {
                    b.Property<long>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<long>("LogChannel")
                        .HasColumnType("bigint");

                    b.Property<bool>("ShouldLog")
                        .HasColumnType("boolean");

                    b.HasKey("GuildId");

                    b.ToTable("JailConfigModels");
                });

            modelBuilder.Entity("RoleManager.Model.JailDataStorage", b =>
                {
                    b.Property<long>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("JailDatas");
                });

            modelBuilder.Entity("RoleManager.Model.ReactionRoleStorageModel", b =>
                {
                    b.Property<long>("GuildId")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<long>("ChannelId")
                        .HasColumnType("bigint");

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.HasKey("GuildId", "Name");

                    b.ToTable("ReactionRoleModels");
                });

            modelBuilder.Entity("RoleManager.Model.ReactionRuleModelBase", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("ReactionRoleStorageModelGuildId")
                        .HasColumnType("bigint");

                    b.Property<string>("ReactionRoleStorageModelName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ReactionRoleStorageModelGuildId", "ReactionRoleStorageModelName")
                        .IsUnique();

                    b.ToTable("ReactionRuleModelBase");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ReactionRuleModelBase");
                });

            modelBuilder.Entity("RoleManager.Model.RoleEventStorageModel", b =>
                {
                    b.Property<Guid>("StorageKey")
                        .HasColumnType("uuid");

                    b.Property<long>("User")
                        .HasColumnType("bigint");

                    b.HasKey("StorageKey", "User");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("RoleManager.Model.ReactionRuleModel", b =>
                {
                    b.HasBaseType("RoleManager.Model.ReactionRuleModelBase");

                    b.Property<string>("Reactions")
                        .HasColumnType("text");

                    b.HasDiscriminator().HasValue("ReactionRuleModel");
                });

            modelBuilder.Entity("RoleManager.Model.ReverseRuleModel", b =>
                {
                    b.HasBaseType("RoleManager.Model.ReactionRuleModelBase");

                    b.Property<string>("Emote")
                        .HasColumnType("text");

                    b.HasDiscriminator().HasValue("ReverseRuleModel");
                });

            modelBuilder.Entity("RoleManager.Model.JailConfigStorageModel", b =>
                {
                    b.OwnsOne("RoleManager.Model.RoleManageStorageModel", "Roles", b1 =>
                        {
                            b1.Property<long>("JailConfigStorageModelGuildId")
                                .HasColumnType("bigint");

                            b1.Property<List<long>>("Add")
                                .HasColumnType("bigint[]");

                            b1.Property<List<long>>("Remove")
                                .HasColumnType("bigint[]");

                            b1.HasKey("JailConfigStorageModelGuildId");

                            b1.ToTable("JailConfigModels");

                            b1.WithOwner()
                                .HasForeignKey("JailConfigStorageModelGuildId");
                        });

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("RoleManager.Model.JailDataStorage", b =>
                {
                    b.OwnsOne("RoleManager.Model.RoleManageStorageModel", "Roles", b1 =>
                        {
                            b1.Property<long>("JailDataStorageGuildId")
                                .HasColumnType("bigint");

                            b1.Property<long>("JailDataStorageUserId")
                                .HasColumnType("bigint");

                            b1.Property<List<long>>("Add")
                                .HasColumnType("bigint[]");

                            b1.Property<List<long>>("Remove")
                                .HasColumnType("bigint[]");

                            b1.HasKey("JailDataStorageGuildId", "JailDataStorageUserId");

                            b1.ToTable("JailDatas");

                            b1.WithOwner()
                                .HasForeignKey("JailDataStorageGuildId", "JailDataStorageUserId");
                        });

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("RoleManager.Model.ReactionRuleModelBase", b =>
                {
                    b.HasOne("RoleManager.Model.ReactionRoleStorageModel", null)
                        .WithOne("Rule")
                        .HasForeignKey("RoleManager.Model.ReactionRuleModelBase", "ReactionRoleStorageModelGuildId", "ReactionRoleStorageModelName");

                    b.OwnsOne("RoleManager.Model.ReactionRoleConfig", "Config", b1 =>
                        {
                            b1.Property<Guid>("ReactionRuleModelBaseId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<bool>("ShouldLogResult")
                                .HasColumnType("boolean");

                            b1.Property<bool>("ShouldRemoveReaction")
                                .HasColumnType("boolean");

                            b1.Property<bool>("ShouldStoreData")
                                .HasColumnType("boolean");

                            b1.Property<Guid>("StorageKey")
                                .HasColumnType("uuid");

                            b1.HasKey("ReactionRuleModelBaseId");

                            b1.ToTable("ReactionRuleModelBase");

                            b1.WithOwner()
                                .HasForeignKey("ReactionRuleModelBaseId");
                        });

                    b.Navigation("Config");
                });

            modelBuilder.Entity("RoleManager.Model.RoleEventStorageModel", b =>
                {
                    b.OwnsOne("RoleManager.Model.RoleManageStorageModel", "RolesChanged", b1 =>
                        {
                            b1.Property<Guid>("RoleEventStorageModelStorageKey")
                                .HasColumnType("uuid");

                            b1.Property<long>("RoleEventStorageModelUser")
                                .HasColumnType("bigint");

                            b1.Property<List<long>>("Add")
                                .HasColumnType("bigint[]");

                            b1.Property<List<long>>("Remove")
                                .HasColumnType("bigint[]");

                            b1.HasKey("RoleEventStorageModelStorageKey", "RoleEventStorageModelUser");

                            b1.ToTable("Events");

                            b1.WithOwner()
                                .HasForeignKey("RoleEventStorageModelStorageKey", "RoleEventStorageModelUser");
                        });

                    b.Navigation("RolesChanged");
                });

            modelBuilder.Entity("RoleManager.Model.ReactionRoleStorageModel", b =>
                {
                    b.Navigation("Rule");
                });
#pragma warning restore 612, 618
        }
    }
}

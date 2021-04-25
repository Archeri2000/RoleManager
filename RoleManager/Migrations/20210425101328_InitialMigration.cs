using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleManager.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_iReactionRuleModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Config_ShouldRemoveReaction = table.Column<bool>(type: "boolean", nullable: true),
                    Config_ShouldLogResult = table.Column<bool>(type: "boolean", nullable: true),
                    Config_ShouldStoreData = table.Column<bool>(type: "boolean", nullable: true),
                    Config_StorageKey = table.Column<Guid>(type: "uuid", nullable: true),
                    Config_Name = table.Column<string>(type: "text", nullable: true),
                    rule_type = table.Column<string>(type: "text", nullable: false),
                    Reactions = table.Column<string>(type: "text", nullable: true),
                    Emote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__iReactionRuleModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    StorageKey = table.Column<Guid>(type: "uuid", nullable: false),
                    User = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RolesChanged_Add = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true),
                    RolesChanged_Remove = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.StorageKey, x.User });
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigModels",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LogChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Roles = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigModels", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "JailConfigModels",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ShouldLog = table.Column<bool>(type: "boolean", nullable: false),
                    LogChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Roles_Add = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true),
                    Roles_Remove = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JailConfigModels", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "JailDatas",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Roles_User = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Roles_RolesChanged_Add = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true),
                    Roles_RolesChanged_Remove = table.Column<List<ulong>>(type: "numeric(20,0)[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JailDatas", x => new { x.GuildId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "ReactionRoleModels",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionRoleModels", x => new { x.GuildId, x.Name });
                    table.ForeignKey(
                        name: "FK_ReactionRoleModels__iReactionRuleModels_RuleId",
                        column: x => x.RuleId,
                        principalTable: "_iReactionRuleModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReactionRoleModels_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "GuildConfigModels");

            migrationBuilder.DropTable(
                name: "JailConfigModels");

            migrationBuilder.DropTable(
                name: "JailDatas");

            migrationBuilder.DropTable(
                name: "ReactionRoleModels");

            migrationBuilder.DropTable(
                name: "_iReactionRuleModels");
        }
    }
}

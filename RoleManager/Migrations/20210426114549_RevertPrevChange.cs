using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleManager.Migrations
{
    public partial class RevertPrevChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRuleModelBase_ReactionRoleModels_ReactionRoleStorag~",
                table: "ReactionRuleModelBase");

            migrationBuilder.DropIndex(
                name: "IX_ReactionRuleModelBase_ReactionRoleStorageModelGuildId_React~",
                table: "ReactionRuleModelBase");

            migrationBuilder.DropColumn(
                name: "ReactionRoleStorageModelGuildId",
                table: "ReactionRuleModelBase");

            migrationBuilder.DropColumn(
                name: "ReactionRoleStorageModelName",
                table: "ReactionRuleModelBase");

            migrationBuilder.AddColumn<Guid>(
                name: "RuleId",
                table: "ReactionRoleModels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactionRoleModels_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                principalTable: "ReactionRuleModelBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.DropIndex(
                name: "IX_ReactionRoleModels_RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.AddColumn<long>(
                name: "ReactionRoleStorageModelGuildId",
                table: "ReactionRuleModelBase",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReactionRoleStorageModelName",
                table: "ReactionRuleModelBase",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactionRuleModelBase_ReactionRoleStorageModelGuildId_React~",
                table: "ReactionRuleModelBase",
                columns: new[] { "ReactionRoleStorageModelGuildId", "ReactionRoleStorageModelName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRuleModelBase_ReactionRoleModels_ReactionRoleStorag~",
                table: "ReactionRuleModelBase",
                columns: new[] { "ReactionRoleStorageModelGuildId", "ReactionRoleStorageModelName" },
                principalTable: "ReactionRoleModels",
                principalColumns: new[] { "GuildId", "Name" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}

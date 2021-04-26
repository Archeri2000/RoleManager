using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleManager.Migrations
{
    public partial class Refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "RuleId",
                table: "ReactionRoleModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                principalTable: "ReactionRuleModelBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "RuleId",
                table: "ReactionRoleModels",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleModels_ReactionRuleModelBase_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                principalTable: "ReactionRuleModelBase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

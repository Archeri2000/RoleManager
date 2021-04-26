using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace RoleManager.Migrations
{
    public partial class removebaseclass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReactionRoleModels__iReactionRuleModels_RuleId",
                table: "ReactionRoleModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK__iReactionRuleModels",
                table: "_iReactionRuleModels");

            migrationBuilder.RenameTable(
                name: "_iReactionRuleModels",
                newName: "ReactionRuleModelBase");

            migrationBuilder.RenameColumn(
                name: "rule_type",
                table: "ReactionRuleModelBase",
                newName: "Discriminator");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "JailConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReactionRuleModelBase",
                table: "ReactionRuleModelBase",
                column: "Id");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReactionRuleModelBase",
                table: "ReactionRuleModelBase");

            migrationBuilder.RenameTable(
                name: "ReactionRuleModelBase",
                newName: "_iReactionRuleModels");

            migrationBuilder.RenameColumn(
                name: "Discriminator",
                table: "_iReactionRuleModels",
                newName: "rule_type");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "JailConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK__iReactionRuleModels",
                table: "_iReactionRuleModels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionRoleModels__iReactionRuleModels_RuleId",
                table: "ReactionRoleModels",
                column: "RuleId",
                principalTable: "_iReactionRuleModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

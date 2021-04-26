using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace RoleManager.Migrations
{
    public partial class Refactor1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles_User",
                table: "JailDatas");

            migrationBuilder.RenameColumn(
                name: "Roles_RolesChanged_Remove",
                table: "JailDatas",
                newName: "Roles_Remove");

            migrationBuilder.RenameColumn(
                name: "Roles_RolesChanged_Add",
                table: "JailDatas",
                newName: "Roles_Add");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "ReactionRoleModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "ReactionRoleModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "ReactionRoleModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "JailDatas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "JailDatas",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<List<long>>(
                name: "Roles_Remove",
                table: "JailDatas",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<long>>(
                name: "Roles_Add",
                table: "JailDatas",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<long>>(
                name: "Roles_Remove",
                table: "JailConfigModels",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<long>>(
                name: "Roles_Add",
                table: "JailConfigModels",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogChannel",
                table: "JailConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "JailConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<List<long>>(
                name: "Roles",
                table: "GuildConfigModels",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LogChannel",
                table: "GuildConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "GuildConfigModels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<List<long>>(
                name: "RolesChanged_Remove",
                table: "Events",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<long>>(
                name: "RolesChanged_Add",
                table: "Events",
                type: "bigint[]",
                nullable: true,
                oldClrType: typeof(decimal[]),
                oldType: "numeric(20,0)[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "User",
                table: "Events",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Roles_Remove",
                table: "JailDatas",
                newName: "Roles_RolesChanged_Remove");

            migrationBuilder.RenameColumn(
                name: "Roles_Add",
                table: "JailDatas",
                newName: "Roles_RolesChanged_Add");

            migrationBuilder.AlterColumn<decimal>(
                name: "MessageId",
                table: "ReactionRoleModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "ChannelId",
                table: "ReactionRoleModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "ReactionRoleModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "UserId",
                table: "JailDatas",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "JailDatas",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal[]>(
                name: "Roles_RolesChanged_Remove",
                table: "JailDatas",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "Roles_RolesChanged_Add",
                table: "JailDatas",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Roles_User",
                table: "JailDatas",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "Roles_Remove",
                table: "JailConfigModels",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "Roles_Add",
                table: "JailConfigModels",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LogChannel",
                table: "JailConfigModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "JailConfigModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "Roles",
                table: "GuildConfigModels",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LogChannel",
                table: "GuildConfigModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "GuildConfigModels",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "RolesChanged_Remove",
                table: "Events",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal[]>(
                name: "RolesChanged_Add",
                table: "Events",
                type: "numeric(20,0)[]",
                nullable: true,
                oldClrType: typeof(List<long>),
                oldType: "bigint[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "User",
                table: "Events",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}

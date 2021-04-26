using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleManager.Migrations
{
    public partial class RefactAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Roles_Remove",
                table: "JailDatas",
                newName: "Remove");

            migrationBuilder.RenameColumn(
                name: "Roles_Add",
                table: "JailDatas",
                newName: "Add");

            migrationBuilder.RenameColumn(
                name: "Roles_Remove",
                table: "JailConfigModels",
                newName: "Remove");

            migrationBuilder.RenameColumn(
                name: "Roles_Add",
                table: "JailConfigModels",
                newName: "Add");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Remove",
                table: "JailDatas",
                newName: "Roles_Remove");

            migrationBuilder.RenameColumn(
                name: "Add",
                table: "JailDatas",
                newName: "Roles_Add");

            migrationBuilder.RenameColumn(
                name: "Remove",
                table: "JailConfigModels",
                newName: "Roles_Remove");

            migrationBuilder.RenameColumn(
                name: "Add",
                table: "JailConfigModels",
                newName: "Roles_Add");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleManager.Migrations
{
    public partial class Flatten : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RolesChanged_Remove",
                table: "Events",
                newName: "Remove");

            migrationBuilder.RenameColumn(
                name: "RolesChanged_Add",
                table: "Events",
                newName: "Add");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Remove",
                table: "Events",
                newName: "RolesChanged_Remove");

            migrationBuilder.RenameColumn(
                name: "Add",
                table: "Events",
                newName: "RolesChanged_Add");
        }
    }
}

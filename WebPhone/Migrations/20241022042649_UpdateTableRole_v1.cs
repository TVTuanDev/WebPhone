using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPhone.Migrations
{
    public partial class UpdateTableRole_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleName",
                table: "Roles");
        }
    }
}

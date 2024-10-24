using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPhone.Migrations
{
    public partial class UpdateTableInventory_v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportPrice",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "Inventories");
        }
    }
}

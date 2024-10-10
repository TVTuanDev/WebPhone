using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPhone.Migrations
{
    public partial class UpdateTableCateProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdParent",
                table: "CategoryProducts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProducts_IdParent",
                table: "CategoryProducts",
                column: "IdParent");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryProducts_CategoryProducts_IdParent",
                table: "CategoryProducts",
                column: "IdParent",
                principalTable: "CategoryProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryProducts_CategoryProducts_IdParent",
                table: "CategoryProducts");

            migrationBuilder.DropIndex(
                name: "IX_CategoryProducts_IdParent",
                table: "CategoryProducts");

            migrationBuilder.DropColumn(
                name: "IdParent",
                table: "CategoryProducts");
        }
    }
}

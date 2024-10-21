using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPhone.Migrations
{
    public partial class AddTriggerForBill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER SetNullWhenDeleteUser
                ON Users
                FOR DELETE
                AS
                BEGIN
                    UPDATE Bills SET CustomerId = NULL
                    WHERE CustomerId IN (SELECT Id From DELETED)

                    UPDATE Bills SET EmploymentId = NULL
                    WHERE EmploymentId IN (SELECT Id From DELETED)
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa trigger khi rollback migration
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS SetNullWhenDeleteUser");
        }
    }
}

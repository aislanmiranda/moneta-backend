using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeScheduleCron : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleCron",
                table: "Employees",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleCron",
                table: "Employees");
        }
    }
}

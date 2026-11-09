using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentTaskTypeDropEmployeeScheduleCron : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleCron",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "AgentTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "AgentTasks");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleCron",
                table: "Employees",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskScheduleStructuredFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                table: "TaskSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hours",
                table: "TaskSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "TaskSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "TaskSchedules");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "TaskSchedules");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "TaskSchedules");
        }
    }
}

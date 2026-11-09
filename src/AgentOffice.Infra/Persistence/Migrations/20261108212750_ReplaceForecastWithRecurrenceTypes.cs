using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceForecastWithRecurrenceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForecastEndDate",
                table: "DateControlEntries");

            migrationBuilder.DropColumn(
                name: "ForecastStartDate",
                table: "DateControlEntries");

            migrationBuilder.DropColumn(
                name: "IsForecast",
                table: "DateControlEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ForecastEndDate",
                table: "DateControlEntries",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ForecastStartDate",
                table: "DateControlEntries",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForecast",
                table: "DateControlEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

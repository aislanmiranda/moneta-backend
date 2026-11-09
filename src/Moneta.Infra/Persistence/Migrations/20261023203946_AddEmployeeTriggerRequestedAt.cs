using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTriggerRequestedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TriggerRequestedAt",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerRequestedAt",
                table: "Employees");
        }
    }
}

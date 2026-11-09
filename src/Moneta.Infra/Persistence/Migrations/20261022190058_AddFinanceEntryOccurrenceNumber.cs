using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceEntryOccurrenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OccurrenceNumber",
                table: "FinanceEntries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccurrenceNumber",
                table: "FinanceEntries");
        }
    }
}

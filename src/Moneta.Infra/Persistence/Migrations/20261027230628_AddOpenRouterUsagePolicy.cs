using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenRouterUsagePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FreeModel",
                table: "AiProviders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastCallUsedFreeModel",
                table: "AiProviders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UsagePolicy",
                table: "AiProviders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeModel",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "LastCallUsedFreeModel",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "UsagePolicy",
                table: "AiProviders");
        }
    }
}

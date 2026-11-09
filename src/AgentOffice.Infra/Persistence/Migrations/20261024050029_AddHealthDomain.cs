using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHealthDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthPersons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    HealthPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CronExpression = table.Column<string>(type: "text", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: true),
                    Hours = table.Column<string>(type: "text", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthItems_HealthPersons_HealthPersonId",
                        column: x => x.HealthPersonId,
                        principalTable: "HealthPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HealthLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DoneAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthLogs_HealthItems_HealthItemId",
                        column: x => x.HealthItemId,
                        principalTable: "HealthItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthItems_HealthPersonId",
                table: "HealthItems",
                column: "HealthPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthLogs_HealthItemId",
                table: "HealthLogs",
                column: "HealthItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthLogs");

            migrationBuilder.DropTable(
                name: "HealthItems");

            migrationBuilder.DropTable(
                name: "HealthPersons");
        }
    }
}

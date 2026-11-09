using System;
using AgentOffice.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Hand-edited after scaffolding: the scaffolder defaulted to DropTable+CreateTable for every
    /// renamed entity (FinanceEntry→DateControlEntry etc), which would have deleted every existing
    /// lançamento/item de saúde. Rewritten as RenameTable/RenameColumn/AddColumn + backfill instead,
    /// so pre-existing data survives, remapped onto the new generic Module-scoped shape. Two fixed
    /// Module rows (ModuleSeedIds.Financeiro/VidaSaudavel) stand in for what used to be the implicit
    /// Finance/Health domains — every pre-existing row gets ModuleId backfilled to one of them.
    ///
    /// Not renamed: the auto-generated PK/FK/index constraint names on the renamed tables (e.g.
    /// "PK_CostCenters", "FK_FinanceEntries_CostCenters_CostCenterId") keep their pre-rename names.
    /// Postgres doesn't rename them automatically on RenameTable/RenameColumn, and leaving them as-is
    /// is purely cosmetic (EF's migration generator compares against the model snapshot, not live
    /// constraint names, so this doesn't affect future `dotnet ef migrations add` runs).
    /// </remarks>
    public partial class GeneralizeModulesDateTimeControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Module catalog ---------------------------------------------------------------

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Pattern = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Slug",
                table: "Modules",
                column: "Slug",
                unique: true);

            // Stand-ins for the two domains that were hardcoded before this migration. Pattern: 0 =
            // DateControl, 1 = TimeControl (ModulePattern declaration order in ModuleEnums.cs).
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name", "Slug", "Icon", "Pattern", "CreatedAt" },
                values: new object[] { ModuleSeedIds.Financeiro, "Financeiro", "financeiro", "Wallet", 0, DateTimeOffset.UtcNow });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name", "Slug", "Icon", "Pattern", "CreatedAt" },
                values: new object[] { ModuleSeedIds.VidaSaudavel, "Vida Saudável", "vida-saudavel", "HeartPulse", 1, DateTimeOffset.UtcNow });

            // --- DateControl (era Financeiro) --------------------------------------------------

            migrationBuilder.RenameTable(name: "CostCenters", newName: "DateControlParties");
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "DateControlParties",
                type: "uuid",
                nullable: false,
                defaultValue: ModuleSeedIds.Financeiro);
            migrationBuilder.CreateIndex(name: "IX_DateControlParties_ModuleId", table: "DateControlParties", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_DateControlParties_Modules_ModuleId",
                table: "DateControlParties",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameTable(name: "FinanceAccounts", newName: "DateControlSources");
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "DateControlSources",
                type: "uuid",
                nullable: false,
                defaultValue: ModuleSeedIds.Financeiro);
            migrationBuilder.CreateIndex(name: "IX_DateControlSources_ModuleId", table: "DateControlSources", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_DateControlSources_Modules_ModuleId",
                table: "DateControlSources",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameTable(name: "FinanceEntries", newName: "DateControlEntries");
            migrationBuilder.RenameColumn(name: "CostCenterId", table: "DateControlEntries", newName: "DateControlPartyId");
            migrationBuilder.RenameColumn(name: "FinanceAccountId", table: "DateControlEntries", newName: "DateControlSourceId");
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "DateControlEntries",
                type: "uuid",
                nullable: false,
                defaultValue: ModuleSeedIds.Financeiro);
            migrationBuilder.CreateIndex(name: "IX_DateControlEntries_ModuleId", table: "DateControlEntries", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_DateControlEntries_Modules_ModuleId",
                table: "DateControlEntries",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // --- TimeControl (era Vida Saudável) ------------------------------------------------

            migrationBuilder.RenameTable(name: "HealthPersons", newName: "TimeControlParties");
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "TimeControlParties",
                type: "uuid",
                nullable: false,
                defaultValue: ModuleSeedIds.VidaSaudavel);
            migrationBuilder.CreateIndex(name: "IX_TimeControlParties_ModuleId", table: "TimeControlParties", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_TimeControlParties_Modules_ModuleId",
                table: "TimeControlParties",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameTable(name: "HealthItems", newName: "TimeControlItems");
            migrationBuilder.RenameColumn(name: "HealthPersonId", table: "TimeControlItems", newName: "TimeControlPartyId");
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "TimeControlItems",
                type: "uuid",
                nullable: false,
                defaultValue: ModuleSeedIds.VidaSaudavel);
            migrationBuilder.CreateIndex(name: "IX_TimeControlItems_ModuleId", table: "TimeControlItems", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_TimeControlItems_Modules_ModuleId",
                table: "TimeControlItems",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameTable(name: "HealthLogs", newName: "TimeControlOccurrences");
            migrationBuilder.RenameColumn(name: "HealthItemId", table: "TimeControlOccurrences", newName: "TimeControlItemId");

            // --- DateTimeControl (novo, Agenda) -------------------------------------------------

            migrationBuilder.CreateTable(
                name: "DateTimeControlEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateTimeControlEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateTimeControlEntries_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateTimeControlEntries_ModuleId",
                table: "DateTimeControlEntries",
                column: "ModuleId");

            // --- AgentTask -> Module -------------------------------------------------------------

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "AgentTasks",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            // Existing rows: Type 0 was FinanceReminder (now DateControlReminder, same ordinal),
            // Type 1 was HealthReminder (now TimeControlReminder, same ordinal) — backfill by the
            // value already stored, no AgentTaskType rows existed for the new DateTimeControl (2).
            migrationBuilder.Sql(
                $"UPDATE \"AgentTasks\" SET \"ModuleId\" = '{ModuleSeedIds.Financeiro}' WHERE \"Type\" = 0;");
            migrationBuilder.Sql(
                $"UPDATE \"AgentTasks\" SET \"ModuleId\" = '{ModuleSeedIds.VidaSaudavel}' WHERE \"Type\" = 1;");

            migrationBuilder.CreateIndex(name: "IX_AgentTasks_ModuleId", table: "AgentTasks", column: "ModuleId");
            migrationBuilder.AddForeignKey(
                name: "FK_AgentTasks_Modules_ModuleId",
                table: "AgentTasks",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_AgentTasks_Modules_ModuleId", table: "AgentTasks");
            migrationBuilder.DropIndex(name: "IX_AgentTasks_ModuleId", table: "AgentTasks");
            migrationBuilder.DropColumn(name: "ModuleId", table: "AgentTasks");

            migrationBuilder.DropTable(name: "DateTimeControlEntries");

            migrationBuilder.RenameColumn(name: "TimeControlItemId", table: "TimeControlOccurrences", newName: "HealthItemId");
            migrationBuilder.RenameTable(name: "TimeControlOccurrences", newName: "HealthLogs");

            migrationBuilder.DropForeignKey(name: "FK_TimeControlItems_Modules_ModuleId", table: "TimeControlItems");
            migrationBuilder.DropIndex(name: "IX_TimeControlItems_ModuleId", table: "TimeControlItems");
            migrationBuilder.DropColumn(name: "ModuleId", table: "TimeControlItems");
            migrationBuilder.RenameColumn(name: "TimeControlPartyId", table: "TimeControlItems", newName: "HealthPersonId");
            migrationBuilder.RenameTable(name: "TimeControlItems", newName: "HealthItems");

            migrationBuilder.DropForeignKey(name: "FK_TimeControlParties_Modules_ModuleId", table: "TimeControlParties");
            migrationBuilder.DropIndex(name: "IX_TimeControlParties_ModuleId", table: "TimeControlParties");
            migrationBuilder.DropColumn(name: "ModuleId", table: "TimeControlParties");
            migrationBuilder.RenameTable(name: "TimeControlParties", newName: "HealthPersons");

            migrationBuilder.DropForeignKey(name: "FK_DateControlEntries_Modules_ModuleId", table: "DateControlEntries");
            migrationBuilder.DropIndex(name: "IX_DateControlEntries_ModuleId", table: "DateControlEntries");
            migrationBuilder.DropColumn(name: "ModuleId", table: "DateControlEntries");
            migrationBuilder.RenameColumn(name: "DateControlSourceId", table: "DateControlEntries", newName: "FinanceAccountId");
            migrationBuilder.RenameColumn(name: "DateControlPartyId", table: "DateControlEntries", newName: "CostCenterId");
            migrationBuilder.RenameTable(name: "DateControlEntries", newName: "FinanceEntries");

            migrationBuilder.DropForeignKey(name: "FK_DateControlSources_Modules_ModuleId", table: "DateControlSources");
            migrationBuilder.DropIndex(name: "IX_DateControlSources_ModuleId", table: "DateControlSources");
            migrationBuilder.DropColumn(name: "ModuleId", table: "DateControlSources");
            migrationBuilder.RenameTable(name: "DateControlSources", newName: "FinanceAccounts");

            migrationBuilder.DropForeignKey(name: "FK_DateControlParties_Modules_ModuleId", table: "DateControlParties");
            migrationBuilder.DropIndex(name: "IX_DateControlParties_ModuleId", table: "DateControlParties");
            migrationBuilder.DropColumn(name: "ModuleId", table: "DateControlParties");
            migrationBuilder.RenameTable(name: "DateControlParties", newName: "CostCenters");

            migrationBuilder.DropTable(name: "Modules");
        }
    }
}

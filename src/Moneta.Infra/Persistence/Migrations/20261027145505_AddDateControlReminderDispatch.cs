using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Also rewrites AgentTasks.Payload for existing DateControlReminder (Type = 0) tasks that
    /// still hold the pre-this-change shape ({"leadDays": N}) into the new ReminderRulesDto shape
    /// — otherwise DateControlReminderJob would read an all-defaults ReminderRulesDto from it
    /// (unknown JSON properties are just ignored) and silently stop reminding for a task that used
    /// to work. The lead-days window becomes BeforeDueDays at a default 09:00, plus OnDueDate
    /// true — a superset of the old behavior (which only reminded ahead of time, never exactly on
    /// the due date), no AfterDueRepeatMinutes since that concept didn't exist before.
    /// </remarks>
    public partial class AddDateControlReminderDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DateControlReminderDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateControlEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKind = table.Column<int>(type: "integer", nullable: false),
                    FiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateControlReminderDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateControlReminderDispatches_DateControlEntries_DateContro~",
                        column: x => x.DateControlEntryId,
                        principalTable: "DateControlEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateControlReminderDispatches_DateControlEntryId_RuleKind_F~",
                table: "DateControlReminderDispatches",
                columns: new[] { "DateControlEntryId", "RuleKind", "FiredAt" });

            migrationBuilder.Sql(
                """
                UPDATE "AgentTasks"
                SET "Payload" = jsonb_build_object(
                    'beforeDueDays', ("Payload"::jsonb->>'leadDays')::int,
                    'beforeDueTime', '09:00',
                    'onDueDate', true,
                    'afterDueRepeatMinutes', NULL
                )::text
                WHERE "Type" = 0
                  AND "Payload" IS NOT NULL
                  AND "Payload"::jsonb ? 'leadDays'
                  AND NOT ("Payload"::jsonb ? 'beforeDueDays');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateControlReminderDispatches");
        }
    }
}

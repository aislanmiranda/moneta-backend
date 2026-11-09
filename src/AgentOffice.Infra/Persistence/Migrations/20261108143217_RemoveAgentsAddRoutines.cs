using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgentsAddRoutines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TaskExecutions_ExecutionId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "DateControlEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Routines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CronExpression = table.Column<string>(type: "text", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: true),
                    Hours = table.Column<string>(type: "text", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TriggerRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routines_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoutineExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutineId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutineExecutions_Routines_RoutineId",
                        column: x => x.RoutineId,
                        principalTable: "Routines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExecutions_RoutineId",
                table: "RoutineExecutions",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_ModuleId",
                table: "Routines",
                column: "ModuleId");

            // Carry every AgentTask (with its single TaskSchedule) over as a Routine, keeping the
            // same ids so TaskExecutions -> RoutineExecutions and their Notifications stay linked.
            // AgentTaskStatus.Active (1) -> RoutineStatus.Active (0); anything else -> Paused (1).
            // A task without a schedule can't become a routine (cron is required) — its
            // executions and notifications are dropped along with it.
            migrationBuilder.Sql("""
                INSERT INTO "Routines" ("Id", "Title", "ModuleId", "Type", "Payload", "Status",
                    "CronExpression", "TimeZone", "DaysOfWeek", "Hours", "Minute", "NextRunAt",
                    "TriggerRequestedAt", "CreatedAt")
                SELECT t."Id", LEFT(t."Title", 200), t."ModuleId", t."Type", t."Payload",
                    CASE WHEN t."Status" = 1 THEN 0 ELSE 1 END,
                    s."CronExpression", s."TimeZone", s."DaysOfWeek", s."Hours", s."Minute", s."NextRunAt",
                    NULL, now()
                FROM "AgentTasks" t
                JOIN LATERAL (
                    SELECT * FROM "TaskSchedules" ts WHERE ts."TaskId" = t."Id" LIMIT 1
                ) s ON true;

                INSERT INTO "RoutineExecutions" ("Id", "RoutineId", "StartedAt", "FinishedAt", "Status", "Result")
                SELECT e."Id", e."TaskId", e."StartedAt", e."FinishedAt", e."Status", e."Result"
                FROM "TaskExecutions" e
                WHERE e."TaskId" IN (SELECT "Id" FROM "Routines");

                DELETE FROM "Notifications"
                WHERE "ExecutionId" NOT IN (SELECT "Id" FROM "RoutineExecutions");
                """);

            migrationBuilder.DropTable(
                name: "AiProviders");

            migrationBuilder.DropTable(
                name: "EmployeeCapabilities");

            migrationBuilder.DropTable(
                name: "TaskExecutions");

            migrationBuilder.DropTable(
                name: "TaskSchedules");

            migrationBuilder.DropTable(
                name: "ToolConnections");

            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropTable(
                name: "AgentTasks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_RoutineExecutions_ExecutionId",
                table: "Notifications",
                column: "ExecutionId",
                principalTable: "RoutineExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_RoutineExecutions_ExecutionId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "RoutineExecutions");

            migrationBuilder.DropTable(
                name: "Routines");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "DateControlEntries");

            migrationBuilder.CreateTable(
                name: "AiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FreeModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastCallUsedFreeModel = table.Column<bool>(type: "boolean", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    UsagePolicy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ToolType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExecutionMode = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Responsibilities = table.Column<string[]>(type: "text[]", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TriggerRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialRef = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolConnections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentTasks_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentTasks_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCapabilities",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permissions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCapabilities", x => new { x.EmployeeId, x.CapabilityId });
                    table.ForeignKey(
                        name: "FK_EmployeeCapabilities_Capabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "Capabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeCapabilities_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskExecutions_AgentTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "AgentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CronExpression = table.Column<string>(type: "text", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: true),
                    Hours = table.Column<string>(type: "text", nullable: true),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSchedules_AgentTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "AgentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_EmployeeId",
                table: "AgentTasks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_ModuleId",
                table: "AgentTasks",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCapabilities_CapabilityId",
                table: "EmployeeCapabilities",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutions_TaskId",
                table: "TaskExecutions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedules_TaskId",
                table: "TaskSchedules",
                column: "TaskId");

            // Down doesn't restore employees/tasks data — notifications pointing at executions
            // that no longer exist would block the FK below.
            migrationBuilder.Sql("""DELETE FROM "Notifications";""");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TaskExecutions_ExecutionId",
                table: "Notifications",
                column: "ExecutionId",
                principalTable: "TaskExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

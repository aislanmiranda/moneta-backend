using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentOffice.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ToolType = table.Column<string>(type: "text", nullable: false),
                    Configuration = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExecutionMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinanceAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialRef = table.Column<string>(type: "text", nullable: true),
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
                    Title = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "FinanceEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinanceAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InstallmentsCount = table.Column<int>(type: "integer", nullable: true),
                    RecurrenceType = table.Column<int>(type: "integer", nullable: false),
                    RecurrenceCount = table.Column<int>(type: "integer", nullable: true),
                    IsForecast = table.Column<bool>(type: "boolean", nullable: false),
                    ForecastStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ForecastEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceEntries_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceEntries_FinanceAccounts_FinanceAccountId",
                        column: x => x.FinanceAccountId,
                        principalTable: "FinanceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true)
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
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_TaskExecutions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "TaskExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentTasks_EmployeeId",
                table: "AgentTasks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCapabilities_CapabilityId",
                table: "EmployeeCapabilities",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceEntries_CostCenterId",
                table: "FinanceEntries",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceEntries_FinanceAccountId",
                table: "FinanceEntries",
                column: "FinanceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExecutionId",
                table: "Notifications",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutions_TaskId",
                table: "TaskExecutions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSchedules_TaskId",
                table: "TaskSchedules",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeCapabilities");

            migrationBuilder.DropTable(
                name: "FinanceEntries");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TaskSchedules");

            migrationBuilder.DropTable(
                name: "ToolConnections");

            migrationBuilder.DropTable(
                name: "Capabilities");

            migrationBuilder.DropTable(
                name: "CostCenters");

            migrationBuilder.DropTable(
                name: "FinanceAccounts");

            migrationBuilder.DropTable(
                name: "TaskExecutions");

            migrationBuilder.DropTable(
                name: "AgentTasks");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}

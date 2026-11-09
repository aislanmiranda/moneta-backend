using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditableCategoriesAndTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DateControlCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateControlCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateControlCategories_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeControlTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeControlTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeControlTypes_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "TimeControlTypeId",
                table: "TimeControlItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DateControlCategoryId",
                table: "DateControlSources",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Each existing module gets the lists that used to be hardcoded (same names/icons as
            // ModuleDefaults, inlined so this migration doesn't change if the defaults do), and
            // every source/item points at the row matching its old enum ordinal.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE legacy_categories (ord int, name text, icon text) ON COMMIT DROP;
                INSERT INTO legacy_categories VALUES
                    (0, 'Internet', 'Wifi'), (1, 'Energia', 'Zap'), (2, 'Água', 'Droplet'),
                    (3, 'Cartão', 'CreditCard'), (4, 'Assinatura', 'Tv'), (5, 'Aluguel', 'Home'),
                    (6, 'Outro', 'MoreHorizontal'), (7, 'Saúde', 'Stethoscope'), (8, 'Lazer', 'Palmtree');

                INSERT INTO "DateControlCategories" ("Id", "ModuleId", "Name", "Icon", "CreatedAt")
                SELECT gen_random_uuid(), m."Id", l.name, l.icon, now()
                FROM "Modules" m CROSS JOIN legacy_categories l
                WHERE m."Pattern" = 0;

                UPDATE "DateControlSources" s
                SET "DateControlCategoryId" = c."Id"
                FROM legacy_categories l
                JOIN "DateControlCategories" c ON c."Name" = l.name
                WHERE c."ModuleId" = s."ModuleId" AND l.ord = s."Category";

                CREATE TEMP TABLE legacy_types (ord int, name text, icon text) ON COMMIT DROP;
                INSERT INTO legacy_types VALUES (0, 'Remédio', 'Pill'), (1, 'Água', 'Droplet'), (2, 'Academia', 'Dumbbell');

                INSERT INTO "TimeControlTypes" ("Id", "ModuleId", "Name", "Icon", "CreatedAt")
                SELECT gen_random_uuid(), m."Id", l.name, l.icon, now()
                FROM "Modules" m CROSS JOIN legacy_types l
                WHERE m."Pattern" = 1;

                UPDATE "TimeControlItems" i
                SET "TimeControlTypeId" = t."Id"
                FROM legacy_types l
                JOIN "TimeControlTypes" t ON t."Name" = l.name
                WHERE t."ModuleId" = i."ModuleId" AND l.ord = i."Type";
                """);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TimeControlItems");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "DateControlSources");

            migrationBuilder.CreateIndex(
                name: "IX_TimeControlItems_TimeControlTypeId",
                table: "TimeControlItems",
                column: "TimeControlTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DateControlSources_DateControlCategoryId",
                table: "DateControlSources",
                column: "DateControlCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DateControlCategories_ModuleId",
                table: "DateControlCategories",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeControlTypes_ModuleId",
                table: "TimeControlTypes",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DateControlSources_DateControlCategories_DateControlCategor~",
                table: "DateControlSources",
                column: "DateControlCategoryId",
                principalTable: "DateControlCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeControlItems_TimeControlTypes_TimeControlTypeId",
                table: "TimeControlItems",
                column: "TimeControlTypeId",
                principalTable: "TimeControlTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateControlSources_DateControlCategories_DateControlCategor~",
                table: "DateControlSources");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeControlItems_TimeControlTypes_TimeControlTypeId",
                table: "TimeControlItems");

            migrationBuilder.DropTable(
                name: "DateControlCategories");

            migrationBuilder.DropTable(
                name: "TimeControlTypes");

            migrationBuilder.DropIndex(
                name: "IX_TimeControlItems_TimeControlTypeId",
                table: "TimeControlItems");

            migrationBuilder.DropIndex(
                name: "IX_DateControlSources_DateControlCategoryId",
                table: "DateControlSources");

            migrationBuilder.DropColumn(
                name: "TimeControlTypeId",
                table: "TimeControlItems");

            migrationBuilder.DropColumn(
                name: "DateControlCategoryId",
                table: "DateControlSources");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TimeControlItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "DateControlSources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

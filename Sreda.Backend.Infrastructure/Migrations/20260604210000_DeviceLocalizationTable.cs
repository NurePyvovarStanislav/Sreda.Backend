using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sreda.Backend.Infrastructure.Persistence;

#nullable disable

namespace Sreda.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SredaDbContext))]
    [Migration("20260604210000_DeviceLocalizationTable")]
    public class DeviceLocalizationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceLocalizedLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLocalizedLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceLocalizedLabels_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLocalizedLabels_DeviceId_Language",
                table: "DeviceLocalizedLabels",
                columns: new[] { "DeviceId", "Language" },
                unique: true);

            migrationBuilder.Sql(
                """
                DO $migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = 'public' AND table_name = 'Devices' AND column_name = 'NameUk'
                    ) THEN
                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'uk', d."NameUk", d."LocationUk"
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'uk');

                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'ru', d."NameRu", d."LocationRu"
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'ru');

                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'en', d."NameEn", d."LocationEn"
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'en');

                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "NameUk";
                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "NameRu";
                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "NameEn";
                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "LocationUk";
                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "LocationRu";
                        ALTER TABLE "Devices" DROP COLUMN IF EXISTS "LocationEn";
                    ELSE
                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'uk', d."Name", d."Location"
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'uk');

                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'ru',
                            CASE WHEN d."Code" = 'Light-1' THEN 'Основной свет' ELSE d."Name" END,
                            CASE WHEN d."Code" = 'Light-1' THEN 'Комната' ELSE d."Location" END
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'ru');

                        INSERT INTO "DeviceLocalizedLabels" ("DeviceId", "Language", "Name", "Location")
                        SELECT d."Id", 'en',
                            CASE WHEN d."Code" = 'Light-1' THEN 'Main light' ELSE d."Name" END,
                            CASE WHEN d."Code" = 'Light-1' THEN 'Room' ELSE d."Location" END
                        FROM "Devices" d
                        WHERE NOT EXISTS (
                            SELECT 1 FROM "DeviceLocalizedLabels" l
                            WHERE l."DeviceId" = d."Id" AND l."Language" = 'en');
                    END IF;
                END $migration$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameUk",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationUk",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationRu",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationEn",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Devices" d SET
                    "NameUk" = COALESCE(uk."Name", d."Name"),
                    "NameRu" = COALESCE(ru."Name", d."Name"),
                    "NameEn" = COALESCE(en."Name", d."Name"),
                    "LocationUk" = uk."Location",
                    "LocationRu" = ru."Location",
                    "LocationEn" = en."Location"
                FROM "Devices" base
                LEFT JOIN "DeviceLocalizedLabels" uk ON uk."DeviceId" = base."Id" AND uk."Language" = 'uk'
                LEFT JOIN "DeviceLocalizedLabels" ru ON ru."DeviceId" = base."Id" AND ru."Language" = 'ru'
                LEFT JOIN "DeviceLocalizedLabels" en ON en."DeviceId" = base."Id" AND en."Language" = 'en'
                WHERE d."Id" = base."Id";
                """);

            migrationBuilder.DropTable(name: "DeviceLocalizedLabels");
        }
    }
}

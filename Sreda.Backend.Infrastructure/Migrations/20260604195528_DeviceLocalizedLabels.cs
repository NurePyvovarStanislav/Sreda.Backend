using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sreda.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeviceLocalizedLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameUk",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

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
                UPDATE "Devices"
                SET
                    "NameUk" = "Name",
                    "NameRu" = CASE WHEN "Code" = 'Light-1' THEN 'Основной свет' ELSE "Name" END,
                    "NameEn" = CASE WHEN "Code" = 'Light-1' THEN 'Main light' ELSE "Name" END,
                    "LocationUk" = "Location",
                    "LocationRu" = CASE WHEN "Code" = 'Light-1' THEN 'Комната' ELSE "Location" END,
                    "LocationEn" = CASE WHEN "Code" = 'Light-1' THEN 'Room' ELSE "Location" END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "NameUk",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameRu",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "Devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NameUk", table: "Devices");
            migrationBuilder.DropColumn(name: "NameRu", table: "Devices");
            migrationBuilder.DropColumn(name: "NameEn", table: "Devices");
            migrationBuilder.DropColumn(name: "LocationUk", table: "Devices");
            migrationBuilder.DropColumn(name: "LocationRu", table: "Devices");
            migrationBuilder.DropColumn(name: "LocationEn", table: "Devices");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccionYCooldownToUmbralSensor_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccionConfigJson",
                table: "UmbralesSensor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccionTipo",
                table: "UmbralesSensor",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CooldownMinutos",
                table: "UmbralesSensor",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoDisparo",
                table: "UmbralesSensor",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccionConfigJson",
                table: "UmbralesSensor");

            migrationBuilder.DropColumn(
                name: "AccionTipo",
                table: "UmbralesSensor");

            migrationBuilder.DropColumn(
                name: "CooldownMinutos",
                table: "UmbralesSensor");

            migrationBuilder.DropColumn(
                name: "UltimoDisparo",
                table: "UmbralesSensor");
        }
    }
}

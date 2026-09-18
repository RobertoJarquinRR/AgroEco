using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaToDateTimeTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Triggers");

            migrationBuilder.AddColumn<DateTime>(
                name: "_fecha",
                table: "DateTimeTrigger",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_fecha",
                table: "DateTimeTrigger");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Triggers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

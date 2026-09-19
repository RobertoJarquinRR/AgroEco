using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaDirectToDateTimeTrigger2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "DateTimeTrigger",
                newName: "TargetTime");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DateTimeTrigger",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DateTimeTrigger");

            migrationBuilder.RenameColumn(
                name: "TargetTime",
                table: "DateTimeTrigger",
                newName: "Fecha");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixFechaPropertyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "_fecha",
                table: "DateTimeTrigger",
                newName: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "DateTimeTrigger",
                newName: "_fecha");
        }
    }
}

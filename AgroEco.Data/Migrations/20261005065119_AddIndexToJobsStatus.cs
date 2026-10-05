using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToJobsStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_Status",
                table: "Jobs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class cambio2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Jobs_Id",
                table: "Actions");

            migrationBuilder.AddColumn<int>(
                name: "Jobid",
                table: "Actions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Jobid",
                table: "Actions",
                column: "Jobid");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Jobs_Jobid",
                table: "Actions",
                column: "Jobid",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Jobs_Jobid",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_Jobid",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "Jobid",
                table: "Actions");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Jobs_Id",
                table: "Actions",
                column: "Id",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

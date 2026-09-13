using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class cambio3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Jobs_Jobid",
                table: "Actions");

            migrationBuilder.RenameColumn(
                name: "Jobid",
                table: "Actions",
                newName: "JobId");

            migrationBuilder.RenameIndex(
                name: "IX_Actions_Jobid",
                table: "Actions",
                newName: "IX_Actions_JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Jobs_JobId",
                table: "Actions",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Jobs_JobId",
                table: "Actions");

            migrationBuilder.RenameColumn(
                name: "JobId",
                table: "Actions",
                newName: "Jobid");

            migrationBuilder.RenameIndex(
                name: "IX_Actions_JobId",
                table: "Actions",
                newName: "IX_Actions_Jobid");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Jobs_Jobid",
                table: "Actions",
                column: "Jobid",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

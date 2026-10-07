using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertStatusAndDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AlertDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlertId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChannelType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AlertEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertDeliveries_Alerts_AlertEntityId",
                        column: x => x.AlertEntityId,
                        principalTable: "Alerts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Status",
                table: "Alerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AlertDeliveries_AlertEntityId",
                table: "AlertDeliveries",
                column: "AlertEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertDeliveries_AlertId",
                table: "AlertDeliveries",
                column: "AlertId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertDeliveries_ChannelType",
                table: "AlertDeliveries",
                column: "ChannelType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertDeliveries");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Status",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Alerts");
        }
    }
}

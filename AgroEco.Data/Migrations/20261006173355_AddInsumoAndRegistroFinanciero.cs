using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInsumoAndRegistroFinanciero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Cultivo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StockMin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Caducidad = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Finca = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Cultivo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaCreacion = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosFinancieros", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_Categoria",
                table: "Insumos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_Nombre",
                table: "Insumos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFinancieros_Fecha",
                table: "RegistrosFinancieros",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFinancieros_TaskId",
                table: "RegistrosFinancieros",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFinancieros_Tipo",
                table: "RegistrosFinancieros",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "RegistrosFinancieros");
        }
    }
}

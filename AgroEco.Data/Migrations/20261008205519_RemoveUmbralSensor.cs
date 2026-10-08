using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUmbralSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UmbralesSensor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UmbralesSensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccionConfigJson = table.Column<string>(type: "TEXT", nullable: true),
                    AccionSugerida = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AccionTipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CantidadInsumoSugerida = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CooldownMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    CostoUnitarioSugerido = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FincaNombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GenerarTareaAuto = table.Column<bool>(type: "INTEGER", nullable: false),
                    InsumoSugeridoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Maximo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Minimo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SensorTipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SeveridadMaxima = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SeveridadMinima = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UltimoDisparo = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmbralesSensor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UmbralesSensor_Activo",
                table: "UmbralesSensor",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_UmbralesSensor_FincaId",
                table: "UmbralesSensor",
                column: "FincaId");

            migrationBuilder.CreateIndex(
                name: "IX_UmbralesSensor_SensorTipo",
                table: "UmbralesSensor",
                column: "SensorTipo");
        }
    }
}

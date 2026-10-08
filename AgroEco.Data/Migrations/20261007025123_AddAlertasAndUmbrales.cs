using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroEco.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertasAndUmbrales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Severidad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FincaNombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SensorId = table.Column<int>(type: "INTEGER", nullable: true),
                    SensorNombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SensorTipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ValorActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UmbralConfigurado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EsActiva = table.Column<bool>(type: "INTEGER", nullable: false),
                    TareaGenerada = table.Column<bool>(type: "INTEGER", nullable: false),
                    TareaGeneradaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaResuelta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaUltimaNotificacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AccionSugerida = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InsumoSugeridoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CantidadInsumoSugerida = table.Column<decimal>(type: "TEXT", nullable: true),
                    CostoUnitarioSugerido = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UmbralesSensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SensorTipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FincaNombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Minimo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Maximo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SeveridadMinima = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SeveridadMaxima = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    GenerarTareaAuto = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccionSugerida = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InsumoSugeridoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CantidadInsumoSugerida = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostoUnitarioSugerido = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmbralesSensor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_EsActiva",
                table: "Alertas",
                column: "EsActiva");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_FechaCreacion",
                table: "Alertas",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_FincaId",
                table: "Alertas",
                column: "FincaId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_SensorId",
                table: "Alertas",
                column: "SensorId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "UmbralesSensor");
        }
    }
}

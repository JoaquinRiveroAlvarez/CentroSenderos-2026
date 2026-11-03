using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class LiqHistorialPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PagosLiquidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RegistradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnuladoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosLiquidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosLiquidaciones_Liquidaciones_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagoLiquidacionImputaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PagoLiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    DetalleLiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    MontoImputado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadSesiones = table.Column<int>(type: "integer", nullable: false),
                    Nota = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoLiquidacionImputaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagoLiquidacionImputaciones_DetalleLiquidaciones_DetalleLiq~",
                        column: x => x.DetalleLiquidacionId,
                        principalTable: "DetalleLiquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagoLiquidacionImputaciones_PagosLiquidaciones_PagoLiquidac~",
                        column: x => x.PagoLiquidacionId,
                        principalTable: "PagosLiquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagoLiquidacionMetodos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PagoLiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    MetodoPagoNombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CuentaOrigen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagoLiquidacionMetodos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagoLiquidacionMetodos_PagosLiquidaciones_PagoLiquidacionId",
                        column: x => x.PagoLiquidacionId,
                        principalTable: "PagosLiquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagoLiquidacionImputaciones_DetalleLiquidacionId",
                table: "PagoLiquidacionImputaciones",
                column: "DetalleLiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PagoLiquidacionImputaciones_PagoLiquidacionId",
                table: "PagoLiquidacionImputaciones",
                column: "PagoLiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PagoLiquidacionMetodos_PagoLiquidacionId",
                table: "PagoLiquidacionMetodos",
                column: "PagoLiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosLiquidaciones_LiquidacionId",
                table: "PagosLiquidaciones",
                column: "LiquidacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagoLiquidacionImputaciones");

            migrationBuilder.DropTable(
                name: "PagoLiquidacionMetodos");

            migrationBuilder.DropTable(
                name: "PagosLiquidaciones");
        }
    }
}

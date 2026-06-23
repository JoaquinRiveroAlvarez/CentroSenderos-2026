using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class Inicio3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "TipoGasto_Tipo_UQ  ",
                table: "TipoGastos",
                newName: "TipoGasto_Tipo_UQ");

            migrationBuilder.CreateTable(
                name: "Gasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TipoGastoId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gasto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gasto_TipoGastos_TipoGastoId",
                        column: x => x.TipoGastoId,
                        principalTable: "TipoGastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Socio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfesionalId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Socio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Socio_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GastoSocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GastoId = table.Column<int>(type: "int", nullable: false),
                    SocioId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastoSocio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GastoSocio_Gasto_GastoId",
                        column: x => x.GastoId,
                        principalTable: "Gasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GastoSocio_Socio_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Liquidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroLiquidacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SocioId = table.Column<int>(type: "int", nullable: false),
                    ProfesionalId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liquidacion_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Liquidacion_Socio_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleLiquidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MontoFacturacion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Retencion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaTurno = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    ProfesionalId = table.Column<int>(type: "int", nullable: false),
                    LiquidacionId = table.Column<int>(type: "int", nullable: false),
                    TipoModalidadId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleLiquidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Liquidacion_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacion_TipoModalidades_TipoModalidadId",
                        column: x => x.TipoModalidadId,
                        principalTable: "TipoModalidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_LiquidacionId",
                table: "DetalleLiquidacion",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_PacienteId",
                table: "DetalleLiquidacion",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_ProfesionalId",
                table: "DetalleLiquidacion",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacion_TipoModalidadId",
                table: "DetalleLiquidacion",
                column: "TipoModalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_TipoGastoId",
                table: "Gasto",
                column: "TipoGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_GastoSocio_GastoId",
                table: "GastoSocio",
                column: "GastoId");

            migrationBuilder.CreateIndex(
                name: "IX_GastoSocio_SocioId",
                table: "GastoSocio",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_ProfesionalId",
                table: "Liquidacion",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidacion_SocioId",
                table: "Liquidacion",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Socio_ProfesionalId",
                table: "Socio",
                column: "ProfesionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleLiquidacion");

            migrationBuilder.DropTable(
                name: "GastoSocio");

            migrationBuilder.DropTable(
                name: "Liquidacion");

            migrationBuilder.DropTable(
                name: "Gasto");

            migrationBuilder.DropTable(
                name: "Socio");

            migrationBuilder.RenameIndex(
                name: "TipoGasto_Tipo_UQ",
                table: "TipoGastos",
                newName: "TipoGasto_Tipo_UQ  ");
        }
    }
}

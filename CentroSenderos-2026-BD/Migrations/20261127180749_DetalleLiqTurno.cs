using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class DetalleLiqTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RegistradoPor",
                table: "PagosLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnuladoPor",
                table: "PagosLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "PagosLiquidaciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Modificado",
                table: "PagosLiquidaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "PagosLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoModificacion",
                table: "PagosLiquidaciones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PagoOriginalId",
                table: "PagosLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadSesiones",
                table: "PagoLiquidacionImputaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "SocioId",
                table: "Liquidaciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioResponsable",
                table: "Liquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadMedioModuloOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadModuloCompletoOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoMedioModuloOriginal",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoModuloCompletoOriginal",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoUnitarioOriginal",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacienteIdOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PacienteNombreOriginal",
                table: "DetalleLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeOriginal",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumenCambiosManuales",
                table: "DetalleLiquidaciones",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SinCoincidenciaAgenda",
                table: "DetalleLiquidaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TipoModalidadIdOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoObraSocialIdOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoObraSocialNombreOriginal",
                table: "DetalleLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPrestacionIdOriginal",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPrestacionNombreOriginal",
                table: "DetalleLiquidaciones",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ValoresModificadosManualmente",
                table: "DetalleLiquidaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DetalleLiquidacionTurnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DetalleLiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    PacienteId = table.Column<int>(type: "integer", nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleLiquidacionTurnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacionTurnos_DetalleLiquidaciones_DetalleLiquid~",
                        column: x => x.DetalleLiquidacionId,
                        principalTable: "DetalleLiquidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacionTurnos_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleLiquidacionTurnos_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosLiquidaciones_PagoOriginalId",
                table: "PagosLiquidaciones",
                column: "PagoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacionTurnos_DetalleLiquidacionId_TurnoId_Pacie~",
                table: "DetalleLiquidacionTurnos",
                columns: new[] { "DetalleLiquidacionId", "TurnoId", "PacienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacionTurnos_PacienteId",
                table: "DetalleLiquidacionTurnos",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidacionTurnos_TurnoId",
                table: "DetalleLiquidacionTurnos",
                column: "TurnoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PagosLiquidaciones_PagosLiquidaciones_PagoOriginalId",
                table: "PagosLiquidaciones",
                column: "PagoOriginalId",
                principalTable: "PagosLiquidaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagosLiquidaciones_PagosLiquidaciones_PagoOriginalId",
                table: "PagosLiquidaciones");

            migrationBuilder.DropTable(
                name: "DetalleLiquidacionTurnos");

            migrationBuilder.DropIndex(
                name: "IX_PagosLiquidaciones_PagoOriginalId",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "Modificado",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MotivoModificacion",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "PagoOriginalId",
                table: "PagosLiquidaciones");

            migrationBuilder.DropColumn(
                name: "UsuarioResponsable",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "CantidadMedioModuloOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "CantidadModuloCompletoOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "CantidadOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoMedioModuloOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoModuloCompletoOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoUnitarioOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "PacienteIdOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "PacienteNombreOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "PorcentajeOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "ResumenCambiosManuales",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "SinCoincidenciaAgenda",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoModalidadIdOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoObraSocialIdOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoObraSocialNombreOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoPrestacionIdOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoPrestacionNombreOriginal",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "ValoresModificadosManualmente",
                table: "DetalleLiquidaciones");

            migrationBuilder.AlterColumn<string>(
                name: "RegistradoPor",
                table: "PagosLiquidaciones",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnuladoPor",
                table: "PagosLiquidaciones",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CantidadSesiones",
                table: "PagoLiquidacionImputaciones",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "SocioId",
                table: "Liquidaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

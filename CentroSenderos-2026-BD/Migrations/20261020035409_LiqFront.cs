using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class LiqFront : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Profesionales_ProfesionalId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropIndex(
                name: "IX_DetalleLiquidaciones_ProfesionalId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "FechaTurno",
                table: "DetalleLiquidaciones");

            migrationBuilder.RenameColumn(
                name: "ProfesionalId",
                table: "DetalleLiquidaciones",
                newName: "EstadoPago");

            migrationBuilder.AddColumn<int>(
                name: "DetalleLiquidacionId",
                table: "Turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiquidacionId",
                table: "Turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLiquidacion",
                table: "Liquidaciones",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "EstadoPago",
                table: "Liquidaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Liquidaciones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFacturacion",
                table: "Liquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRetencion",
                table: "Liquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSaldo",
                table: "Liquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Retencion",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Porcentaje",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "PacienteId",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFacturacion",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CantidadMedioModulo",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CantidadModuloCompleto",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoMedioModulo",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoModuloCompleto",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoUnitario",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "DetalleLiquidaciones",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Saldo",
                table: "DetalleLiquidaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TipoObraSocialId",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPrestacionId",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_DetalleLiquidacionId",
                table: "Turnos",
                column: "DetalleLiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_LiquidacionId",
                table: "Turnos",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidaciones_TipoObraSocialId",
                table: "DetalleLiquidaciones",
                column: "TipoObraSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidaciones_TipoPrestacionId",
                table: "DetalleLiquidaciones",
                column: "TipoPrestacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_TipoObraSocial_TipoObraSocialId",
                table: "DetalleLiquidaciones",
                column: "TipoObraSocialId",
                principalTable: "TipoObraSocial",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_TipoPrestaciones_TipoPrestacionId",
                table: "DetalleLiquidaciones",
                column: "TipoPrestacionId",
                principalTable: "TipoPrestaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_DetalleLiquidaciones_DetalleLiquidacionId",
                table: "Turnos",
                column: "DetalleLiquidacionId",
                principalTable: "DetalleLiquidaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_Liquidaciones_LiquidacionId",
                table: "Turnos",
                column: "LiquidacionId",
                principalTable: "Liquidaciones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_TipoObraSocial_TipoObraSocialId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_TipoPrestaciones_TipoPrestacionId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_DetalleLiquidaciones_DetalleLiquidacionId",
                table: "Turnos");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_Liquidaciones_LiquidacionId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_DetalleLiquidacionId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_LiquidacionId",
                table: "Turnos");

            migrationBuilder.DropIndex(
                name: "IX_DetalleLiquidaciones_TipoObraSocialId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropIndex(
                name: "IX_DetalleLiquidaciones_TipoPrestacionId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "DetalleLiquidacionId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "LiquidacionId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "TotalFacturacion",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "TotalRetencion",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "TotalSaldo",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "CantidadMedioModulo",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "CantidadModuloCompleto",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoMedioModulo",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoModuloCompleto",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "MontoUnitario",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "Saldo",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoObraSocialId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropColumn(
                name: "TipoPrestacionId",
                table: "DetalleLiquidaciones");

            migrationBuilder.RenameColumn(
                name: "EstadoPago",
                table: "DetalleLiquidaciones",
                newName: "ProfesionalId");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroLiquidacion",
                table: "Liquidaciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<decimal>(
                name: "Retencion",
                table: "DetalleLiquidaciones",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Porcentaje",
                table: "DetalleLiquidaciones",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "PacienteId",
                table: "DetalleLiquidaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoFacturacion",
                table: "DetalleLiquidaciones",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaTurno",
                table: "DetalleLiquidaciones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DetalleLiquidaciones_ProfesionalId",
                table: "DetalleLiquidaciones",
                column: "ProfesionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Profesionales_ProfesionalId",
                table: "DetalleLiquidaciones",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

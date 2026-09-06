using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class TurnosRecurrentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SerieTurnoId",
                table: "Turnos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SeriesTurnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Frecuencia = table.Column<int>(type: "integer", nullable: false),
                    Intervalo = table.Column<int>(type: "integer", nullable: false),
                    UnidadPersonalizada = table.Column<int>(type: "integer", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesTurnos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_SerieTurnoId",
                table: "Turnos",
                column: "SerieTurnoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_SeriesTurnos_SerieTurnoId",
                table: "Turnos",
                column: "SerieTurnoId",
                principalTable: "SeriesTurnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_SeriesTurnos_SerieTurnoId",
                table: "Turnos");

            migrationBuilder.DropTable(
                name: "SeriesTurnos");

            migrationBuilder.DropIndex(
                name: "IX_Turnos_SerieTurnoId",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "SerieTurnoId",
                table: "Turnos");
        }
    }
}

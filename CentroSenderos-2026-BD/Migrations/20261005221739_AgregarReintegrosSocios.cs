using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReintegrosSocios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReintegrosSocios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SocioPagadorId = table.Column<int>(type: "integer", nullable: false),
                    SocioReceptorId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReintegrosSocios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReintegrosSocios_Socios_SocioPagadorId",
                        column: x => x.SocioPagadorId,
                        principalTable: "Socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosSocios_Socios_SocioReceptorId",
                        column: x => x.SocioReceptorId,
                        principalTable: "Socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReintegrosSociosDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReintegroSocioId = table.Column<int>(type: "integer", nullable: false),
                    GastoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReintegrosSociosDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReintegrosSociosDetalles_Gastos_GastoId",
                        column: x => x.GastoId,
                        principalTable: "Gastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReintegrosSociosDetalles_ReintegrosSocios_ReintegroSocioId",
                        column: x => x.ReintegroSocioId,
                        principalTable: "ReintegrosSocios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosSocios_SocioPagadorId",
                table: "ReintegrosSocios",
                column: "SocioPagadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosSocios_SocioReceptorId",
                table: "ReintegrosSocios",
                column: "SocioReceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosSociosDetalles_GastoId",
                table: "ReintegrosSociosDetalles",
                column: "GastoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReintegrosSociosDetalles_ReintegroSocioId_GastoId",
                table: "ReintegrosSociosDetalles",
                columns: new[] { "ReintegroSocioId", "GastoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReintegrosSociosDetalles");

            migrationBuilder.DropTable(
                name: "ReintegrosSocios");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class LiqPrestaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeRetencion",
                table: "TipoPrestaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PorcentajeRetencion",
                table: "TipoPrestaciones");
        }
    }
}

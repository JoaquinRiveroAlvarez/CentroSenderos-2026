using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class RelacionOpcionalPrestacionArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoAreaId",
                table: "TipoPrestaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoPrestaciones_TipoAreaId",
                table: "TipoPrestaciones",
                column: "TipoAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_TipoPrestaciones_TipoAreas_TipoAreaId",
                table: "TipoPrestaciones",
                column: "TipoAreaId",
                principalTable: "TipoAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TipoPrestaciones_TipoAreas_TipoAreaId",
                table: "TipoPrestaciones");

            migrationBuilder.DropIndex(
                name: "IX_TipoPrestaciones_TipoAreaId",
                table: "TipoPrestaciones");

            migrationBuilder.DropColumn(
                name: "TipoAreaId",
                table: "TipoPrestaciones");
        }
    }
}

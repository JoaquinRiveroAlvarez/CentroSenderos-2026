using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class TipoPrestacionProfesional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoPrestacionId",
                table: "Profesionales",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profesionales_TipoPrestacionId",
                table: "Profesionales",
                column: "TipoPrestacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profesionales_TipoPrestaciones_TipoPrestacionId",
                table: "Profesionales",
                column: "TipoPrestacionId",
                principalTable: "TipoPrestaciones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profesionales_TipoPrestaciones_TipoPrestacionId",
                table: "Profesionales");

            migrationBuilder.DropIndex(
                name: "IX_Profesionales_TipoPrestacionId",
                table: "Profesionales");

            migrationBuilder.DropColumn(
                name: "TipoPrestacionId",
                table: "Profesionales");
        }
    }
}

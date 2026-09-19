using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class AsociarPrestacionConProfesionalEnTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfesionalId",
                table: "TurnoTipoPrestaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TurnoTipoPrestaciones_ProfesionalId",
                table: "TurnoTipoPrestaciones",
                column: "ProfesionalId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnoTipoPrestaciones_TurnoId_ProfesionalId",
                table: "TurnoTipoPrestaciones",
                columns: new[]
                {
            "TurnoId",
            "ProfesionalId"
                },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestaciones_Profesionales_ProfesionalId",
                table: "TurnoTipoPrestaciones",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestaciones_Profesionales_ProfesionalId",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.DropIndex(
                name: "IX_TurnoTipoPrestaciones_ProfesionalId",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.DropIndex(
                name: "IX_TurnoTipoPrestaciones_TurnoId_ProfesionalId",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.DropColumn(
                name: "ProfesionalId",
                table: "TurnoTipoPrestaciones");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class prestacionenturno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestacion_TipoPrestaciones_TipoPrestacionId",
                table: "TurnoTipoPrestacion");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestacion_Turnos_TurnoId",
                table: "TurnoTipoPrestacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurnoTipoPrestacion",
                table: "TurnoTipoPrestacion");

            migrationBuilder.RenameTable(
                name: "TurnoTipoPrestacion",
                newName: "TurnoTipoPrestaciones");

            migrationBuilder.RenameIndex(
                name: "IX_TurnoTipoPrestacion_TurnoId",
                table: "TurnoTipoPrestaciones",
                newName: "IX_TurnoTipoPrestaciones_TurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_TurnoTipoPrestacion_TipoPrestacionId",
                table: "TurnoTipoPrestaciones",
                newName: "IX_TurnoTipoPrestaciones_TipoPrestacionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurnoTipoPrestaciones",
                table: "TurnoTipoPrestaciones",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestaciones_TipoPrestaciones_TipoPrestacionId",
                table: "TurnoTipoPrestaciones",
                column: "TipoPrestacionId",
                principalTable: "TipoPrestaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestaciones_Turnos_TurnoId",
                table: "TurnoTipoPrestaciones",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestaciones_TipoPrestaciones_TipoPrestacionId",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestaciones_Turnos_TurnoId",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TurnoTipoPrestaciones",
                table: "TurnoTipoPrestaciones");

            migrationBuilder.RenameTable(
                name: "TurnoTipoPrestaciones",
                newName: "TurnoTipoPrestacion");

            migrationBuilder.RenameIndex(
                name: "IX_TurnoTipoPrestaciones_TurnoId",
                table: "TurnoTipoPrestacion",
                newName: "IX_TurnoTipoPrestacion_TurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_TurnoTipoPrestaciones_TipoPrestacionId",
                table: "TurnoTipoPrestacion",
                newName: "IX_TurnoTipoPrestacion_TipoPrestacionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TurnoTipoPrestacion",
                table: "TurnoTipoPrestacion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestacion_TipoPrestaciones_TipoPrestacionId",
                table: "TurnoTipoPrestacion",
                column: "TipoPrestacionId",
                principalTable: "TipoPrestaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestacion_Turnos_TurnoId",
                table: "TurnoTipoPrestacion",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

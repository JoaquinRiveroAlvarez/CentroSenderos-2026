using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class NullableTipoTurnoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos");

            migrationBuilder.AlterColumn<int>(
                name: "TipoTurnoId",
                table: "Turnos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos",
                column: "TipoTurnoId",
                principalTable: "TipoTurnos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos");

            migrationBuilder.AlterColumn<int>(
                name: "TipoTurnoId",
                table: "Turnos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos",
                column: "TipoTurnoId",
                principalTable: "TipoTurnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

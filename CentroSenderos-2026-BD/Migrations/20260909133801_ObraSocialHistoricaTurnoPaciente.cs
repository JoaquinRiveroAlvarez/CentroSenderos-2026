using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class ObraSocialHistoricaTurnoPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreObraSocial",
                table: "TurnoPaciente",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoObraSocialId",
                table: "TurnoPaciente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurnoPaciente_TipoObraSocialId",
                table: "TurnoPaciente",
                column: "TipoObraSocialId");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoPaciente_TipoObraSocial_TipoObraSocialId",
                table: "TurnoPaciente",
                column: "TipoObraSocialId",
                principalTable: "TipoObraSocial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnoPaciente_TipoObraSocial_TipoObraSocialId",
                table: "TurnoPaciente");

            migrationBuilder.DropIndex(
                name: "IX_TurnoPaciente_TipoObraSocialId",
                table: "TurnoPaciente");

            migrationBuilder.DropColumn(
                name: "NombreObraSocial",
                table: "TurnoPaciente");

            migrationBuilder.DropColumn(
                name: "TipoObraSocialId",
                table: "TurnoPaciente");
        }
    }
}

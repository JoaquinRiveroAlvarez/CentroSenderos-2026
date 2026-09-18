using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentoIdPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentoId",
                table: "Pacientes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Pacientes_Documento_DocumentoId",
            //    table: "Pacientes",
            //    column: "DocumentoId",
            //    principalTable: "Documento",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Pacientes_Documento_DocumentoId",
            //    table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_DocumentoId",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "DocumentoId",
                table: "Pacientes");
        }
    }
}

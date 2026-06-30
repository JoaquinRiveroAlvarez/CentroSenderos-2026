using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class errorpaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentoId",
                table: "Pacientes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId",
                principalTable: "Documento",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentoId",
                table: "Pacientes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId",
                principalTable: "Documento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

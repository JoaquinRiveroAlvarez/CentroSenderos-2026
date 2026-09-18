using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class documento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documento_TipoDocumentos_TipoDocumentoId",
                table: "Documento");

            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_DocumentoId",
                table: "Pacientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documento",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "DocumentoId",
                table: "Pacientes");

            migrationBuilder.RenameTable(
                name: "Documento",
                newName: "Documentos");

            migrationBuilder.RenameIndex(
                name: "IX_Documento_TipoDocumentoId",
                table: "Documentos",
                newName: "IX_Documentos_TipoDocumentoId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSubida",
                table: "Documentos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PacienteId",
                table: "Documentos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UrlArchivo",
                table: "Documentos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documentos",
                table: "Documentos",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_PacienteId",
                table: "Documentos",
                column: "PacienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Pacientes_PacienteId",
                table: "Documentos",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_TipoDocumentos_TipoDocumentoId",
                table: "Documentos",
                column: "TipoDocumentoId",
                principalTable: "TipoDocumentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Pacientes_PacienteId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_TipoDocumentos_TipoDocumentoId",
                table: "Documentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documentos",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_PacienteId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FechaSubida",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "PacienteId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "UrlArchivo",
                table: "Documentos");

            migrationBuilder.RenameTable(
                name: "Documentos",
                newName: "Documento");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_TipoDocumentoId",
                table: "Documento",
                newName: "IX_Documento_TipoDocumentoId");

            migrationBuilder.AddColumn<int>(
                name: "DocumentoId",
                table: "Pacientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documento",
                table: "Documento",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documento_TipoDocumentos_TipoDocumentoId",
                table: "Documento",
                column: "TipoDocumentoId",
                principalTable: "TipoDocumentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Documento_DocumentoId",
                table: "Pacientes",
                column: "DocumentoId",
                principalTable: "Documento",
                principalColumn: "Id");
        }
    }
}

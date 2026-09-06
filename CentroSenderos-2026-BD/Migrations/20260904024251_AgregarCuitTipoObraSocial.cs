using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCuitTipoObraSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cuit",
                table: "TipoObraSocial",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "TipoObraSocial_Cuit_UQ",
                table: "TipoObraSocial",
                column: "Cuit",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "TipoObraSocial_Cuit_UQ",
                table: "TipoObraSocial");

            migrationBuilder.DropColumn(
                name: "Cuit",
                table: "TipoObraSocial");
        }
    }
}

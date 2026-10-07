using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCajaSenderos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProfesionalId",
                table: "Socios",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "EsCaja",
                table: "Socios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "Socio_Caja_Unica",
                table: "Socios",
                column: "EsCaja",
                unique: true,
                filter: "\"EsCaja\" = TRUE");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Socio_Caja_Profesional",
                table: "Socios",
                sql: "(\"EsCaja\" = TRUE AND \"ProfesionalId\" IS NULL) OR (\"EsCaja\" = FALSE AND \"ProfesionalId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Socio_Caja_Unica",
                table: "Socios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Socio_Caja_Profesional",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "EsCaja",
                table: "Socios");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesionalId",
                table: "Socios",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

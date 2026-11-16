using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class MatriculaPorPrestacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Profesional_MP_UQ",
                table: "Profesionales");

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "ProfesionalTipoPrestaciones",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            // Conserva la matrícula anterior en todas las prestaciones ya asignadas.
            migrationBuilder.Sql("""
                UPDATE "ProfesionalTipoPrestaciones" AS relacion
                SET "Matricula" = profesional."MP"
                FROM "Profesionales" AS profesional
                WHERE relacion."ProfesionalId" = profesional."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "ProfesionalTipoPrestaciones");

            migrationBuilder.CreateIndex(
                name: "Profesional_MP_UQ",
                table: "Profesionales",
                column: "MP",
                unique: true);
        }
    }
}

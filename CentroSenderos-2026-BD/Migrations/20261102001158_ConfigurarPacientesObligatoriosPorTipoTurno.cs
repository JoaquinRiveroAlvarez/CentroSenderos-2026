using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarPacientesObligatoriosPorTipoTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequierePacientes",
                table: "TipoTurnos",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            // Configuración inicial:
            // solo Sesión normal requiere pacientes.
            // Después se podrá modificar desde Tipo de turno.
            migrationBuilder.Sql(
                """
        UPDATE "TipoTurnos"
        SET "RequierePacientes" = (
            lower(
                translate(
                    btrim("Tipo"),
                    'ÁÉÍÓÚáéíóú',
                    'AEIOUaeiou'
                )
            ) = 'sesion normal'
        );
        """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequierePacientes",
                table: "TipoTurnos"
            );
        }
    }
}

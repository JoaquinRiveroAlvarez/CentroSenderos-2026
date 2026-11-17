using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    // Solo modifica datos: no cambia el modelo ni requiere actualizar el snapshot.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261117120000_RenombrarSesionIndividual")]
    public class RenombrarSesionIndividual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "TipoTurnos"
                SET "Tipo" = 'Sesión individual'
                WHERE lower(translate(btrim("Tipo"), 'ÁÉÍÓÚáéíóú', 'AEIOUaeiou')) = 'sesion normal';

                UPDATE "TipoTurnos"
                SET "RequierePacientes" = TRUE
                WHERE lower(translate(btrim("Tipo"), 'ÁÉÍÓÚáéíóú', 'AEIOUaeiou')) = 'sesion individual';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "TipoTurnos"
                SET "Tipo" = 'Sesión normal'
                WHERE "Tipo" = 'Sesión individual';
                """);
        }
    }
}

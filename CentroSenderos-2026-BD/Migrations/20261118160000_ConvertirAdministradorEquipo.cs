using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CentroSenderos_2026_BD.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261118160000_ConvertirAdministradorEquipo")]
public sealed class ConvertirAdministradorEquipo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
            SELECT anterior."UserId", admin."Id"
            FROM "AspNetUserRoles" anterior
            JOIN "AspNetRoles" legado ON legado."Id" = anterior."RoleId"
            JOIN "AspNetRoles" admin ON admin."NormalizedName" = 'ADMIN'
            WHERE legado."NormalizedName" = 'ADMINISTRADOREQUIPO'
              AND NOT EXISTS (
                SELECT 1
                FROM "AspNetUserRoles" activo
                JOIN "AspNetRoles" rol ON rol."Id" = activo."RoleId"
                WHERE activo."UserId" = anterior."UserId"
                  AND rol."NormalizedName" IN ('ADMIN', 'SOCIO', 'CALENDARIO', 'EQUIPO', 'PROFESIONAL')
              )
            ON CONFLICT ("UserId", "RoleId") DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Los roles asignados quedan sujetos a la administración normal de cuentas.
    }
}

using CentroSenderos_2026_BD;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CentroSenderos_2026_BD.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261118150000_RolesYPlanillasPrivadas")]
public sealed class RolesYPlanillasPrivadas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            VALUES ('f532b62d-a1f5-41ab-8f3d-688aba565aaa', 'Socio', 'SOCIO', 'f532b62d-a1f5-41ab-8f3d-688aba565aaa'),
                   ('84b6770f-086f-4198-98fe-b071029939e0', 'Calendario', 'CALENDARIO', '84b6770f-086f-4198-98fe-b071029939e0')
            ON CONFLICT ("NormalizedName") DO NOTHING;
            """);
        migrationBuilder.AddColumn<string>(
            name: "PropietarioPrivadoId", table: "Planillas", type: "text", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM "Planillas" WHERE "PropietarioPrivadoId" IS NOT NULL;
            DELETE FROM "AspNetRoles" WHERE "Id" IN
                ('f532b62d-a1f5-41ab-8f3d-688aba565aaa', '84b6770f-086f-4198-98fe-b071029939e0');
            """);
        migrationBuilder.DropColumn(name: "PropietarioPrivadoId", table: "Planillas");
    }
}

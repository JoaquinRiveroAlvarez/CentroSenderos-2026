using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var clave = Guid.NewGuid().ToString();
            var nombreRol = "admin";
            var nombreRolNormalizado = nombreRol.ToUpper();

            migrationBuilder.Sql(
                "INSERT INTO \"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('"
                + clave + "', '" + nombreRol + "', '" + nombreRolNormalizado + "')"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"AspNetRoles\" WHERE \"Name\" = 'admin'");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class RolesTodos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            InsertarRol(migrationBuilder, "admin");
            InsertarRol(migrationBuilder, "equipo");
            InsertarRol(migrationBuilder, "profesional");
        }

        private void InsertarRol(MigrationBuilder migrationBuilder, string nombreRol)
        {
            var clave = Guid.NewGuid().ToString();
            var nombreRolNormalizado = nombreRol.ToUpper();

            migrationBuilder.Sql(
                "INSERT INTO \"AspNetRoles\" (\"Id\", \"Name\", \"NormalizedName\") VALUES ('"
                + clave + "', '" + nombreRol + "', '" + nombreRolNormalizado + "')"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"AspNetRoles\" WHERE \"Name\" IN ('admin','equipo','profesional')");
        }
    }
}

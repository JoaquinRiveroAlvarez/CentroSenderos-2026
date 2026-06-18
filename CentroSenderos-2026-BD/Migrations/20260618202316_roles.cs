using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var clave = Guid.NewGuid().ToString();
            var nombreRol = "admin";
            var NombreRolNormalizado = nombreRol.ToUpper();

            migrationBuilder.Sql(@$"INSERT INTO AspNetRoles(Id, Name, NormalizedName) VALUES ('{clave}', '{nombreRol}', '{NombreRolNormalizado}')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE name = 'admin'");
        }
    }
}

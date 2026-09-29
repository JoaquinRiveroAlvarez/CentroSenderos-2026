using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class TipoArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Observacion = table.Column<string>(type: "text", nullable: false),
                    EstadoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoAreas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "TipoArea_Tipo_UQ",
                table: "TipoAreas",
                column: "Tipo",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "TipoAreas" ("Observacion", "EstadoRegistro", "Tipo", "Descripcion")
                SELECT
                    '',
                    1,
                    MIN(BTRIM("Area")),
                    ''
                FROM "Profesionales"
                WHERE BTRIM("Area") <> ''
                GROUP BY LOWER(BTRIM("Area"));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipoAreas");
        }
    }
}

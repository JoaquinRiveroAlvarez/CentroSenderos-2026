using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    public partial class ProfesionalMultiplesPrestaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero se crea la nueva tabla intermedia.
            migrationBuilder.CreateTable(
                name: "ProfesionalTipoPrestaciones",
                columns: table => new
                {
                    Id = table.Column<int>(
                            type: "integer",
                            nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy
                                .IdentityByDefaultColumn),

                    ProfesionalId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    TipoPrestacionId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    Observacion = table.Column<string>(
                        type: "text",
                        nullable: false),

                    EstadoRegistro = table.Column<int>(
                        type: "integer",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ProfesionalTipoPrestaciones",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ProfesionalTipoPrestaciones_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_ProfesionalTipoPrestaciones_TipoPrestaciones_TipoPrestacion~",
                        column: x => x.TipoPrestacionId,
                        principalTable: "TipoPrestaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfesionalTipoPrestaciones_ProfesionalId_TipoPrestacionId",
                table: "ProfesionalTipoPrestaciones",
                columns: new[]
                {
                    "ProfesionalId",
                    "TipoPrestacionId"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfesionalTipoPrestaciones_TipoPrestacionId",
                table: "ProfesionalTipoPrestaciones",
                column: "TipoPrestacionId");

            // Copia las prestaciones actuales a la tabla intermedia.
            migrationBuilder.Sql(
                """
                INSERT INTO "ProfesionalTipoPrestaciones"
                    (
                        "ProfesionalId",
                        "TipoPrestacionId",
                        "Observacion",
                        "EstadoRegistro"
                    )
                SELECT
                    "Id",
                    "TipoPrestacionId",
                    '',
                    1
                FROM "Profesionales"
                WHERE "TipoPrestacionId" IS NOT NULL;
                """
            );

            // Después de copiar los datos se elimina la relación anterior.
            migrationBuilder.DropForeignKey(
                name: "FK_Profesionales_TipoPrestaciones_TipoPrestacionId",
                table: "Profesionales");

            migrationBuilder.DropIndex(
                name: "IX_Profesionales_TipoPrestacionId",
                table: "Profesionales");

            migrationBuilder.DropColumn(
                name: "TipoPrestacionId",
                table: "Profesionales");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaura la columna anterior.
            migrationBuilder.AddColumn<int>(
                name: "TipoPrestacionId",
                table: "Profesionales",
                type: "integer",
                nullable: true);

            // Si hay varias prestaciones, conserva una al revertir.
            migrationBuilder.Sql(
                """
                UPDATE "Profesionales" AS p
                SET "TipoPrestacionId" =
                (
                    SELECT MIN(ptp."TipoPrestacionId")
                    FROM "ProfesionalTipoPrestaciones" AS ptp
                    WHERE ptp."ProfesionalId" = p."Id"
                );
                """
            );

            migrationBuilder.DropTable(
                name: "ProfesionalTipoPrestaciones");

            migrationBuilder.CreateIndex(
                name: "IX_Profesionales_TipoPrestacionId",
                table: "Profesionales",
                column: "TipoPrestacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profesionales_TipoPrestaciones_TipoPrestacionId",
                table: "Profesionales",
                column: "TipoPrestacionId",
                principalTable: "TipoPrestaciones",
                principalColumn: "Id");
        }
    }
}
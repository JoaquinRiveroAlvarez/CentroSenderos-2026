using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentroSenderos_2026_BD.Migrations
{
    /// <inheritdoc />
    public partial class inicio2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidacion_Liquidacion_LiquidacionId",
                table: "DetalleLiquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidacion_Pacientes_PacienteId",
                table: "DetalleLiquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidacion_Profesionales_ProfesionalId",
                table: "DetalleLiquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidacion_TipoModalidades_TipoModalidadId",
                table: "DetalleLiquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Gasto_TipoGastos_TipoGastoId",
                table: "Gasto");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoSocio_Gasto_GastoId",
                table: "GastoSocio");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoSocio_Socio_SocioId",
                table: "GastoSocio");

            migrationBuilder.DropForeignKey(
                name: "FK_Liquidacion_Profesionales_ProfesionalId",
                table: "Liquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Liquidacion_Socio_SocioId",
                table: "Liquidacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_TipoObrasSociales_TipoObraSocialId",
                table: "Pacientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Socio_Profesionales_ProfesionalId",
                table: "Socio");

            migrationBuilder.DropForeignKey(
                name: "FK_Turno_TipoConsultorios_TipoConsultorioId",
                table: "Turno");

            migrationBuilder.DropForeignKey(
                name: "FK_Turno_TipoTurnos_TipoTurnoId",
                table: "Turno");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoPaciente_Turno_TurnoId",
                table: "TurnoPaciente");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoProfesional_Turno_TurnoId",
                table: "TurnoProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestacion_Turno_TurnoId",
                table: "TurnoTipoPrestacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Turno",
                table: "Turno");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoObrasSociales",
                table: "TipoObrasSociales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Socio",
                table: "Socio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Liquidacion",
                table: "Liquidacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gasto",
                table: "Gasto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleLiquidacion",
                table: "DetalleLiquidacion");

            migrationBuilder.RenameTable(
                name: "Turno",
                newName: "Turnos");

            migrationBuilder.RenameTable(
                name: "TipoObrasSociales",
                newName: "TipoObraSocial");

            migrationBuilder.RenameTable(
                name: "Socio",
                newName: "Socios");

            migrationBuilder.RenameTable(
                name: "Liquidacion",
                newName: "Liquidaciones");

            migrationBuilder.RenameTable(
                name: "Gasto",
                newName: "Gastos");

            migrationBuilder.RenameTable(
                name: "DetalleLiquidacion",
                newName: "DetalleLiquidaciones");

            migrationBuilder.RenameIndex(
                name: "IX_Turno_TipoTurnoId",
                table: "Turnos",
                newName: "IX_Turnos_TipoTurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_Turno_TipoConsultorioId",
                table: "Turnos",
                newName: "IX_Turnos_TipoConsultorioId");

            migrationBuilder.RenameIndex(
                name: "IX_Liquidacion_SocioId",
                table: "Liquidaciones",
                newName: "IX_Liquidaciones_SocioId");

            migrationBuilder.RenameIndex(
                name: "IX_Liquidacion_ProfesionalId",
                table: "Liquidaciones",
                newName: "IX_Liquidaciones_ProfesionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Gasto_TipoGastoId",
                table: "Gastos",
                newName: "IX_Gastos_TipoGastoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidacion_TipoModalidadId",
                table: "DetalleLiquidaciones",
                newName: "IX_DetalleLiquidaciones_TipoModalidadId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidacion_ProfesionalId",
                table: "DetalleLiquidaciones",
                newName: "IX_DetalleLiquidaciones_ProfesionalId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidacion_PacienteId",
                table: "DetalleLiquidaciones",
                newName: "IX_DetalleLiquidaciones_PacienteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidacion_LiquidacionId",
                table: "DetalleLiquidaciones",
                newName: "IX_DetalleLiquidaciones_LiquidacionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Turnos",
                table: "Turnos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoObraSocial",
                table: "TipoObraSocial",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Socios",
                table: "Socios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Liquidaciones",
                table: "Liquidaciones",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gastos",
                table: "Gastos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleLiquidaciones",
                table: "DetalleLiquidaciones",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Liquidaciones_LiquidacionId",
                table: "DetalleLiquidaciones",
                column: "LiquidacionId",
                principalTable: "Liquidaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_Profesionales_ProfesionalId",
                table: "DetalleLiquidaciones",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidaciones_TipoModalidades_TipoModalidadId",
                table: "DetalleLiquidaciones",
                column: "TipoModalidadId",
                principalTable: "TipoModalidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_TipoGastos_TipoGastoId",
                table: "Gastos",
                column: "TipoGastoId",
                principalTable: "TipoGastos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GastoSocio_Gastos_GastoId",
                table: "GastoSocio",
                column: "GastoId",
                principalTable: "Gastos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GastoSocio_Socios_SocioId",
                table: "GastoSocio",
                column: "SocioId",
                principalTable: "Socios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidaciones_Profesionales_ProfesionalId",
                table: "Liquidaciones",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidaciones_Socios_SocioId",
                table: "Liquidaciones",
                column: "SocioId",
                principalTable: "Socios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_TipoObraSocial_TipoObraSocialId",
                table: "Pacientes",
                column: "TipoObraSocialId",
                principalTable: "TipoObraSocial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Profesionales_ProfesionalId",
                table: "Socios",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoPaciente_Turnos_TurnoId",
                table: "TurnoPaciente",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoProfesional_Turnos_TurnoId",
                table: "TurnoProfesional",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_TipoConsultorios_TipoConsultorioId",
                table: "Turnos",
                column: "TipoConsultorioId",
                principalTable: "TipoConsultorios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos",
                column: "TipoTurnoId",
                principalTable: "TipoTurnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestacion_Turnos_TurnoId",
                table: "TurnoTipoPrestacion",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Liquidaciones_LiquidacionId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Pacientes_PacienteId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_Profesionales_ProfesionalId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleLiquidaciones_TipoModalidades_TipoModalidadId",
                table: "DetalleLiquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_TipoGastos_TipoGastoId",
                table: "Gastos");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoSocio_Gastos_GastoId",
                table: "GastoSocio");

            migrationBuilder.DropForeignKey(
                name: "FK_GastoSocio_Socios_SocioId",
                table: "GastoSocio");

            migrationBuilder.DropForeignKey(
                name: "FK_Liquidaciones_Profesionales_ProfesionalId",
                table: "Liquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Liquidaciones_Socios_SocioId",
                table: "Liquidaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_TipoObraSocial_TipoObraSocialId",
                table: "Pacientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Profesionales_ProfesionalId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoPaciente_Turnos_TurnoId",
                table: "TurnoPaciente");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoProfesional_Turnos_TurnoId",
                table: "TurnoProfesional");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_TipoConsultorios_TipoConsultorioId",
                table: "Turnos");

            migrationBuilder.DropForeignKey(
                name: "FK_Turnos_TipoTurnos_TipoTurnoId",
                table: "Turnos");

            migrationBuilder.DropForeignKey(
                name: "FK_TurnoTipoPrestacion_Turnos_TurnoId",
                table: "TurnoTipoPrestacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Turnos",
                table: "Turnos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoObraSocial",
                table: "TipoObraSocial");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Socios",
                table: "Socios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Liquidaciones",
                table: "Liquidaciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gastos",
                table: "Gastos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleLiquidaciones",
                table: "DetalleLiquidaciones");

            migrationBuilder.RenameTable(
                name: "Turnos",
                newName: "Turno");

            migrationBuilder.RenameTable(
                name: "TipoObraSocial",
                newName: "TipoObrasSociales");

            migrationBuilder.RenameTable(
                name: "Socios",
                newName: "Socio");

            migrationBuilder.RenameTable(
                name: "Liquidaciones",
                newName: "Liquidacion");

            migrationBuilder.RenameTable(
                name: "Gastos",
                newName: "Gasto");

            migrationBuilder.RenameTable(
                name: "DetalleLiquidaciones",
                newName: "DetalleLiquidacion");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_TipoTurnoId",
                table: "Turno",
                newName: "IX_Turno_TipoTurnoId");

            migrationBuilder.RenameIndex(
                name: "IX_Turnos_TipoConsultorioId",
                table: "Turno",
                newName: "IX_Turno_TipoConsultorioId");

            migrationBuilder.RenameIndex(
                name: "IX_Liquidaciones_SocioId",
                table: "Liquidacion",
                newName: "IX_Liquidacion_SocioId");

            migrationBuilder.RenameIndex(
                name: "IX_Liquidaciones_ProfesionalId",
                table: "Liquidacion",
                newName: "IX_Liquidacion_ProfesionalId");

            migrationBuilder.RenameIndex(
                name: "IX_Gastos_TipoGastoId",
                table: "Gasto",
                newName: "IX_Gasto_TipoGastoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidaciones_TipoModalidadId",
                table: "DetalleLiquidacion",
                newName: "IX_DetalleLiquidacion_TipoModalidadId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidaciones_ProfesionalId",
                table: "DetalleLiquidacion",
                newName: "IX_DetalleLiquidacion_ProfesionalId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidaciones_PacienteId",
                table: "DetalleLiquidacion",
                newName: "IX_DetalleLiquidacion_PacienteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleLiquidaciones_LiquidacionId",
                table: "DetalleLiquidacion",
                newName: "IX_DetalleLiquidacion_LiquidacionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Turno",
                table: "Turno",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoObrasSociales",
                table: "TipoObrasSociales",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Socio",
                table: "Socio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Liquidacion",
                table: "Liquidacion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gasto",
                table: "Gasto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleLiquidacion",
                table: "DetalleLiquidacion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidacion_Liquidacion_LiquidacionId",
                table: "DetalleLiquidacion",
                column: "LiquidacionId",
                principalTable: "Liquidacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidacion_Pacientes_PacienteId",
                table: "DetalleLiquidacion",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidacion_Profesionales_ProfesionalId",
                table: "DetalleLiquidacion",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleLiquidacion_TipoModalidades_TipoModalidadId",
                table: "DetalleLiquidacion",
                column: "TipoModalidadId",
                principalTable: "TipoModalidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gasto_TipoGastos_TipoGastoId",
                table: "Gasto",
                column: "TipoGastoId",
                principalTable: "TipoGastos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GastoSocio_Gasto_GastoId",
                table: "GastoSocio",
                column: "GastoId",
                principalTable: "Gasto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GastoSocio_Socio_SocioId",
                table: "GastoSocio",
                column: "SocioId",
                principalTable: "Socio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidacion_Profesionales_ProfesionalId",
                table: "Liquidacion",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidacion_Socio_SocioId",
                table: "Liquidacion",
                column: "SocioId",
                principalTable: "Socio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_TipoObrasSociales_TipoObraSocialId",
                table: "Pacientes",
                column: "TipoObraSocialId",
                principalTable: "TipoObrasSociales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Socio_Profesionales_ProfesionalId",
                table: "Socio",
                column: "ProfesionalId",
                principalTable: "Profesionales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turno_TipoConsultorios_TipoConsultorioId",
                table: "Turno",
                column: "TipoConsultorioId",
                principalTable: "TipoConsultorios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Turno_TipoTurnos_TipoTurnoId",
                table: "Turno",
                column: "TipoTurnoId",
                principalTable: "TipoTurnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoPaciente_Turno_TurnoId",
                table: "TurnoPaciente",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoProfesional_Turno_TurnoId",
                table: "TurnoProfesional",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TurnoTipoPrestacion_Turno_TurnoId",
                table: "TurnoTipoPrestacion",
                column: "TurnoId",
                principalTable: "Turno",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

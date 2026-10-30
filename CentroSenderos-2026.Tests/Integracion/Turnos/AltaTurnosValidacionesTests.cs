using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Xunit;

namespace CentroSenderos_2026.Tests.Integracion.Turnos;

public class AltaTurnosValidacionesTests
{
    #region Datos obligatorios

    [Theory]
    [InlineData("hora", "Seleccioná un horario válido.")]
    [InlineData("tipoCero", "Seleccioná un tipo de turno válido.")]
    [InlineData("tipoNegativo", "Seleccioná un tipo de turno válido.")]
    [InlineData("consultorioCero", "Seleccioná un consultorio válido.")]
    [InlineData("consultorioNegativo", "Seleccioná un consultorio válido.")]
    [InlineData("profesionales", "Seleccioná al menos un profesional.")]
    [InlineData("pacientes", "Seleccioná al menos un paciente.")]
    [InlineData("prestaciones", "Seleccioná una prestación para cada profesional.")]
    public async Task Alta_DatosIncompletos_RechazaConMensajeEspecifico(
        string escenario,
        string mensajeEsperado)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        var repositorio = new TurnoRepositorio(contexto);
        var dto = CrearSolicitud();

        // Cada caso altera únicamente el dato que se está probando.
        switch (escenario)
        {
            case "hora":
                dto.Hora = TimeOnly.MinValue;
                break;

            case "tipoCero":
                dto.TipoTurnoId = 0;
                break;

            case "tipoNegativo":
                dto.TipoTurnoId = -1;
                break;

            case "consultorioCero":
                dto.TipoConsultorioId = 0;
                break;

            case "consultorioNegativo":
                dto.TipoConsultorioId = -1;
                break;

            case "profesionales":
                dto.ProfesionalIds.Clear();
                break;

            case "pacientes":
                dto.PacienteIds.Clear();
                break;

            case "prestaciones":
                dto.ProfesionalPrestaciones.Clear();
                break;
        }

        var error = await Assert.ThrowsAsync<ApplicationException>(
            async () => await repositorio.InsertarTurno(dto));

        Assert.Equal(mensajeEsperado, error.Message);
    }

    #endregion

    #region Correspondencia entre profesional y prestación

    [Fact]
    public async Task Alta_PrestacionDeOtroProfesional_RechazaLaSolicitud()
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        var repositorio = new TurnoRepositorio(contexto);
        var dto = CrearSolicitud();

        // Hay una prestación, pero corresponde a un profesional
        // distinto del seleccionado para el turno.
        dto.ProfesionalPrestaciones[0].ProfesionalId = 2;

        var error = await Assert.ThrowsAsync<ApplicationException>(
            async () => await repositorio.InsertarTurno(dto));

        Assert.Equal(
            "Revisá las prestaciones seleccionadas. " +
            "Cada prestación debe corresponder a su profesional.",
            error.Message);
    }

    #endregion

    #region Preparación de solicitudes

    private static TurnoDTO CrearSolicitud()
    {
        // Estos identificadores son valores de entrada sintéticos.
        // Los casos deben rechazarse antes de consultar relaciones
        // o intentar guardar registros en la base.
        return new TurnoDTO
        {
            Fecha = DateTime.Today.AddDays(30),
            Hora = new TimeOnly(10, 0),
            TipoTurnoId = 1,
            TipoConsultorioId = 1,
            ProfesionalIds = new List<int> { 1 },
            PacienteIds = new List<int> { 1 },
            ProfesionalPrestaciones =
                new List<TurnoProfesionalPrestacionDTO>
                {
                    new()
                    {
                        ProfesionalId = 1,
                        TipoPrestacionId = 1
                    }
                }
        };
    }

    #endregion
}
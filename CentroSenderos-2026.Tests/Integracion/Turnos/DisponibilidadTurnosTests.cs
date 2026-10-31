using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests.Integracion.Turnos;

public class DisponibilidadTurnosTests
{
    #region Disponibilidad y superposiciones

    // El turno existente de prueba ocupa de 10:00 a 11:00.
    // Se consulta disponibilidad para atenciones de 30 minutos.
    [Theory]
    [InlineData("consultorio", 10, 0, false)]
    [InlineData("consultorio", 10, 30, false)]
    [InlineData("profesional", 10, 0, false)]
    [InlineData("paciente", 10, 0, false)]
    [InlineData("consultorio", 9, 30, true)]
    [InlineData("consultorio", 11, 0, true)]
    [InlineData("cancelado", 10, 0, true)]
    [InlineData("excluir", 10, 0, true)]
    [InlineData("otros", 10, 0, true)]
    public async Task Disponibilidad_RespetaOcupacionYLimites(
        string escenario,
        int hora,
        int minuto,
        bool debeEstarDisponible)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        try
        {
            #region Preparación

            // Solo tomamos identificadores existentes.
            // No modificamos pacientes ni profesionales.
            var referencia = await contexto.Turnos
                .AsNoTracking()
                .Select(t => new
                {
                    t.TipoConsultorioId
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(referencia);

            var profesionalId = await contexto.Set<TurnoProfesional>()
                .Select(r => r.ProfesionalId)
                .FirstOrDefaultAsync();

            var pacienteId = await contexto.Set<TurnoPaciente>()
                .Select(r => r.PacienteId)
                .FirstOrDefaultAsync();

            Assert.True(profesionalId > 0,
                "La base de testing necesita un profesional asociado a un turno.");
            Assert.True(pacienteId > 0,
                "La base de testing necesita un paciente asociado a un turno.");

            var otroConsultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c => c.Id != referencia.TipoConsultorioId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(otroConsultorioId > 0,
                "La base de testing necesita al menos dos consultorios.");

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-DISP-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            // Buscamos un día futuro sin turnos para aislar el caso.
            var fecha = DateTime.UtcNow.Date.AddYears(2);

            for (var intento = 0; intento < 366; intento++)
            {
                var inicioDia =
                    DateTime.SpecifyKind(fecha, DateTimeKind.Utc);

                var finDia = inicioDia.AddDays(1);

                var ocupado = await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < finDia &&
                    t.FechaFin > inicioDia);

                if (!ocupado)
                    break;

                fecha = fecha.AddDays(1);
            }

            var inicio = DateTime.SpecifyKind(
                fecha.AddHours(10), DateTimeKind.Utc);

            Assert.False(await contexto.Turnos.AnyAsync(t =>
                t.FechaInicio < inicio.Date.AddDays(1) &&
                t.FechaFin > inicio.Date));

            var turno = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = referencia.TipoConsultorioId,
                FechaInicio = inicio,
                FechaFin = inicio.AddHours(1),
                EstadoRegistro = EnumEstadoRegistro.activo,
                EstadoTurno = escenario == "cancelado"
                    ? EnumEstadoTurno.cancelado
                    : EnumEstadoTurno.reservado,
                TurnoProfesionales = new List<TurnoProfesional>
                {
                    new()
                    {
                        ProfesionalId = profesionalId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }
                },
                TurnoPacientes = new List<TurnoPaciente>
                {
                    new()
                    {
                        PacienteId = pacienteId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }
                }
            };

            contexto.Add(turno);
            await contexto.SaveChangesAsync();

            #endregion

            #region Ejecución

            // En profesional/paciente usamos otro consultorio para
            // comprobar cada recurso sin que el consultorio lo oculte.
            var consultorioConsulta =
                escenario is "profesional" or "paciente" or "otros"
                    ? otroConsultorioId
                    : referencia.TipoConsultorioId;

            var profesionales = escenario == "profesional"
                ? new List<int> { profesionalId }
                : new List<int>();

            var pacientes = escenario == "paciente"
                ? new List<int> { pacienteId }
                : new List<int>();

            int? excluir = escenario == "excluir"
                ? turno.Id
                : null;

            var repositorio = new TurnoRepositorio(contexto);

            var disponibles = await repositorio.HorariosDisponibles(
                DateOnly.FromDateTime(fecha),
                tipo.Id,
                consultorioConsulta,
                profesionales,
                pacientes,
                excluir);

            #endregion

            #region Comprobación

            var horario = new TimeOnly(hora, minuto);

            if (debeEstarDisponible)
                Assert.Contains(horario, disponibles);
            else
                Assert.DoesNotContain(horario, disponibles);

            #endregion
        }
        finally
        {
            // Revierte las filas creadas incluso si falla una aserción.
            await transaccion.RollbackAsync();
        }
    }

    #endregion
}
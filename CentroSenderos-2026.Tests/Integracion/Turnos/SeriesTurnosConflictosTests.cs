using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests.Integracion.Turnos;

public class SeriesTurnosConflictosTests
{
    #region Conflictos durante la creación de series

    [Theory]
    [InlineData("consultorio")]
    [InlineData("profesional")]
    [InlineData("paciente")]
    public async Task Serie_ConflictoEnSegundaOcurrencia_RechazaSinGuardar(
        string recurso)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        try
        {
            #region Datos de referencia

            var relacion = await contexto.ProfesionalTipoPrestaciones
                .Where(r =>
                    r.EstadoRegistro == EnumEstadoRegistro.activo &&
                    r.Profesional.EstadoRegistro ==
                        EnumEstadoRegistro.activo &&
                    r.TipoPrestacion.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                .Select(r => new
                {
                    r.ProfesionalId,
                    r.TipoPrestacionId
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(relacion);

            var pacienteId = await contexto.Pacientes
                .Where(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                pacienteId > 0,
                "Se necesita un paciente activo en testing.");

            var consultorios = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .Take(2)
                .ToListAsync();

            Assert.True(
                consultorios.Count == 2,
                "Se necesitan dos consultorios activos en testing.");

            #endregion

            #region Preparación del período

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var periodoLibre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var hasta = desde.AddDays(15);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    periodoLibre = true;
                    break;
                }

                fecha = fecha.AddDays(15);
            }

            Assert.True(
                periodoLibre,
                "No se encontró un período libre para la prueba.");

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-CONFLICTO-SERIE-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            #endregion

            #region Turno ocupado en la segunda ocurrencia

            var inicioOcupado = DateTime.SpecifyKind(
                fecha.AddDays(7).AddHours(10),
                DateTimeKind.Utc);

            var ocupado = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorios[0],
                FechaInicio = inicioOcupado,
                FechaFin = inicioOcupado.AddMinutes(30),
                EstadoTurno = EnumEstadoTurno.reservado,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            // Cada escenario bloquea un único recurso.
            if (recurso == "profesional")
            {
                ocupado.TurnoProfesionales.Add(
                    new TurnoProfesional
                    {
                        ProfesionalId = relacion.ProfesionalId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    });
            }

            if (recurso == "paciente")
            {
                ocupado.TurnoPacientes.Add(
                    new TurnoPaciente
                    {
                        PacienteId = pacienteId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    });
            }

            contexto.Add(ocupado);
            await contexto.SaveChangesAsync();

            var cantidadSeriesAntes =
                await contexto.Set<SerieTurno>().CountAsync();

            #endregion

            #region Solicitud de tres turnos semanales

            var dto = new TurnoDTO
            {
                Fecha = fecha,
                Hora = new TimeOnly(10, 0),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,

                TipoConsultorioId = recurso == "consultorio"
                    ? consultorios[0]
                    : consultorios[1],

                FrecuenciaRecurrencia =
                    EnumFrecuenciaRecurrenciaTurno.semanal,

                FechaHastaRecurrencia = fecha.AddDays(14),
                IntervaloRecurrencia = 1,

                ProfesionalIds = new List<int>
                {
                    relacion.ProfesionalId
                },

                PacienteIds = new List<int>
                {
                    pacienteId
                },

                ProfesionalPrestaciones =
                    new List<TurnoProfesionalPrestacionDTO>
                    {
                        new()
                        {
                            ProfesionalId = relacion.ProfesionalId,
                            TipoPrestacionId = relacion.TipoPrestacionId
                        }
                    }
            };

            var repositorio = new TurnoRepositorio(contexto);

            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => await repositorio.InsertarTurno(dto));

            Assert.Contains(
                recurso,
                error.Message.ToLowerInvariant());

            #endregion

            #region Verificación de que no hubo guardado parcial

            var turnosGuardados = await contexto.Turnos
                .AsNoTracking()
                .Where(t => t.TipoTurnoId == tipo.Id)
                .ToListAsync();

            // Solo debe existir el turno preparado como obstáculo.
            var unico = Assert.Single(turnosGuardados);

            Assert.Equal(ocupado.Id, unico.Id);
            Assert.Equal(inicioOcupado, unico.FechaInicio);

            Assert.Equal(
                inicioOcupado.AddMinutes(30),
                unico.FechaFin);

            Assert.Null(unico.SerieTurnoId);

            Assert.Equal(
                cantidadSeriesAntes,
                await contexto.Set<SerieTurno>().CountAsync());

            // No debe guardarse la primera ocurrencia,
            // aunque su horario estuviera libre.
            var inicioPrimera = DateTime.SpecifyKind(
                fecha.AddHours(10),
                DateTimeKind.Utc);

            Assert.False(await contexto.Turnos.AnyAsync(t =>
                t.TipoTurnoId == tipo.Id &&
                t.FechaInicio == inicioPrimera));

            #endregion
        }
        finally
        {
            #region Limpieza

            await transaccion.RollbackAsync();

            #endregion
        }
    }

    #endregion
}
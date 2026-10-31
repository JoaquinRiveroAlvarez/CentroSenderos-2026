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

public class EdicionSeriesConflictosTests
{
    #region Conflictos al editar una serie

    [Theory]
    [InlineData("consultorio")]
    [InlineData("profesional")]
    [InlineData("paciente")]
    public async Task EditarSerie_ConflictoEnUnaOcurrencia_ConservaTodaLaSerie(
        string recurso)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        try
        {
            #region Referencias y período libre

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

            Assert.True(pacienteId > 0);

            var consultorios = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .Take(2)
                .ToListAsync();

            Assert.Equal(2, consultorios.Count);

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var libre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var hasta = desde.AddDays(15);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    libre = true;
                    break;
                }

                fecha = fecha.AddDays(15);
            }

            Assert.True(libre);

            #endregion

            #region Preparación de la serie

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-EDIT-CONFLICTO-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            var serie = new SerieTurno
            {
                Frecuencia = EnumFrecuenciaRecurrenciaTurno.semanal,
                Intervalo = 1,
                FechaInicio = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc),
                FechaHasta = DateTime.SpecifyKind(
                    fecha.AddDays(14), DateTimeKind.Utc),
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(serie);
            await contexto.SaveChangesAsync();

            var consultorioSerie = recurso == "consultorio"
                ? consultorios[0]
                : consultorios[1];

            var turnos = new List<Turno>();

            for (var i = 0; i < 3; i++)
            {
                var inicio = DateTime.SpecifyKind(
                    fecha.AddDays(i * 7).AddHours(10),
                    DateTimeKind.Utc);

                turnos.Add(new Turno
                {
                    SerieTurnoId = serie.Id,
                    TipoTurnoId = tipo.Id,
                    TipoConsultorioId = consultorioSerie,
                    FechaInicio = inicio,
                    FechaFin = inicio.AddMinutes(30),
                    EstadoTurno = EnumEstadoTurno.reservado,
                    EstadoRegistro = EnumEstadoRegistro.activo,
                    TurnoProfesionales = new List<TurnoProfesional>
                    {
                        new()
                        {
                            ProfesionalId = relacion.ProfesionalId,
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
                    },
                    TurnoTipoPrestaciones =
                        new List<TurnoTipoPrestacion>
                        {
                            new()
                            {
                                ProfesionalId = relacion.ProfesionalId,
                                TipoPrestacionId = relacion.TipoPrestacionId,
                                EstadoRegistro = EnumEstadoRegistro.activo
                            }
                        }
                });
            }

            // Obstáculo a las 12:00 de la segunda fecha.
            var inicioOcupado = DateTime.SpecifyKind(
                fecha.AddDays(7).AddHours(12),
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

            if (recurso == "profesional")
            {
                ocupado.TurnoProfesionales.Add(new TurnoProfesional
                {
                    ProfesionalId = relacion.ProfesionalId,
                    EstadoRegistro = EnumEstadoRegistro.activo
                });
            }

            if (recurso == "paciente")
            {
                ocupado.TurnoPacientes.Add(new TurnoPaciente
                {
                    PacienteId = pacienteId,
                    EstadoRegistro = EnumEstadoRegistro.activo
                });
            }

            contexto.AddRange(turnos);
            contexto.Add(ocupado);
            await contexto.SaveChangesAsync();

            #endregion

            #region Intento de cambiar toda la serie a las 12

            var dto = new TurnoDTO
            {
                Id = turnos[1].Id,
                Fecha = fecha.AddDays(7),
                Hora = new TimeOnly(12, 0),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioSerie,
                SerieTurnoId = serie.Id,
                ModificarTodaLaSerie = true,
                FrecuenciaRecurrencia =
                    EnumFrecuenciaRecurrenciaTurno.semanal,
                FechaHastaRecurrencia = fecha.AddDays(14),
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
                async () => await repositorio.ActualizarTurno(
                    turnos[1].Id, dto));

            Assert.Contains(
                recurso,
                error.Message.ToLowerInvariant());

            #endregion

            #region Verificación de que toda la serie quedó intacta

            var guardados = await contexto.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .Where(t => t.SerieTurnoId == serie.Id)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            Assert.Equal(3, guardados.Count);

            for (var i = 0; i < 3; i++)
            {
                var esperado = DateTime.SpecifyKind(
                    fecha.AddDays(i * 7).AddHours(10),
                    DateTimeKind.Utc);

                var turno = guardados[i];

                Assert.Equal(turnos[i].Id, turno.Id);
                Assert.Equal(esperado, turno.FechaInicio);
                Assert.Equal(
                    esperado.AddMinutes(30),
                    turno.FechaFin);

                Assert.Equal(
                    consultorioSerie,
                    turno.TipoConsultorioId);

                Assert.Equal(
                    relacion.ProfesionalId,
                    Assert.Single(
                        turno.TurnoProfesionales).ProfesionalId);

                Assert.Equal(
                    pacienteId,
                    Assert.Single(
                        turno.TurnoPacientes).PacienteId);

                var prestacion = Assert.Single(
                    turno.TurnoTipoPrestaciones);

                Assert.Equal(
                    relacion.ProfesionalId,
                    prestacion.ProfesionalId);

                Assert.Equal(
                    relacion.TipoPrestacionId,
                    prestacion.TipoPrestacionId);
            }

            Assert.Equal(
                4,
                await contexto.Turnos.CountAsync(t =>
                    t.TipoTurnoId == tipo.Id));

            #endregion
        }
        finally
        {
            // Revierte la serie y el obstáculo preparados.
            await transaccion.RollbackAsync();
        }
    }

    #endregion
}
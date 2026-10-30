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

public class SeriesTurnosHistoricosTests
{
    #region Conservación de ocurrencias pasadas

    [Fact]
    public async Task EditarFuturas_ConservaLaOcurrenciaPasada()
    {
        var tipoPruebaId = 0;
        var seriePruebaId = 0;

        try
        {
            await using var contexto = BaseDePruebas.CrearContexto();

            #region Referencias

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

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(consultorioId > 0);

            #endregion

            #region Preparación de fechas

            var hoy = DateTime.UtcNow.Date;
            var fechaPasada = hoy.AddDays(-7);
            var primeraFutura = hoy.AddDays(7);
            var periodoLibre = false;

            for (var intento = 0; intento < 52; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    primeraFutura, DateTimeKind.Utc);

                var hasta = desde.AddDays(8);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    periodoLibre = true;
                    break;
                }

                primeraFutura = primeraFutura.AddDays(7);
            }

            Assert.True(
                periodoLibre,
                "No se encontró un período futuro libre.");

            var fechas = new[]
            {
                fechaPasada,
                primeraFutura,
                primeraFutura.AddDays(7)
            };

            #endregion

            #region Preparación de la serie histórica

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-HISTORICO-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();
            tipoPruebaId = tipo.Id;

            var serie = new SerieTurno
            {
                Frecuencia = EnumFrecuenciaRecurrenciaTurno.semanal,
                Intervalo = 1,
                FechaInicio = DateTime.SpecifyKind(
                    fechaPasada, DateTimeKind.Utc),
                FechaHasta = DateTime.SpecifyKind(
                    fechas[2], DateTimeKind.Utc),
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(serie);
            await contexto.SaveChangesAsync();
            seriePruebaId = serie.Id;

            // Preparamos directamente una serie con una ocurrencia
            // pasada y dos futuras. No probamos aquí su generación.
            var turnos = fechas.Select(fecha =>
            {
                var inicio = DateTime.SpecifyKind(
                    fecha.AddHours(10), DateTimeKind.Utc);

                return new Turno
                {
                    SerieTurnoId = serie.Id,
                    TipoTurnoId = tipo.Id,
                    TipoConsultorioId = consultorioId,
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
                };
            }).ToList();

            contexto.AddRange(turnos);
            await contexto.SaveChangesAsync();

            var profesionalHistoricoId =
                turnos[0].TurnoProfesionales.Single().Id;

            var pacienteHistoricoId =
                turnos[0].TurnoPacientes.Single().Id;

            var prestacionHistoricaId =
                turnos[0].TurnoTipoPrestaciones.Single().Id;

            #endregion

            #region Edición de las futuras

            var dto = new TurnoDTO
            {
                Id = turnos[1].Id,
                Fecha = primeraFutura,
                Hora = new TimeOnly(12, 0),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioId,
                SerieTurnoId = serie.Id,
                ModificarTodaLaSerie = true,
                FrecuenciaRecurrencia =
                    EnumFrecuenciaRecurrenciaTurno.semanal,
                FechaHastaRecurrencia = fechas[2],
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

            Assert.True(await repositorio.ActualizarTurno(
                turnos[1].Id, dto));

            #endregion

            #region Verificación independiente

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var guardados = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .Where(t => t.SerieTurnoId == seriePruebaId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            Assert.Equal(3, guardados.Count);

            for (var i = 0; i < 3; i++)
            {
                var horaEsperada = i == 0 ? 10 : 12;

                var inicioEsperado = DateTime.SpecifyKind(
                    fechas[i].AddHours(horaEsperada),
                    DateTimeKind.Utc);

                var turno = guardados[i];

                Assert.Equal(turnos[i].Id, turno.Id);
                Assert.Equal(inicioEsperado, turno.FechaInicio);
                Assert.Equal(
                    inicioEsperado.AddMinutes(30),
                    turno.FechaFin);

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

            // Las relaciones de la ocurrencia pasada tampoco
            // deben haberse eliminado y vuelto a crear.
            var pasado = guardados[0];

            Assert.Equal(
                profesionalHistoricoId,
                Assert.Single(pasado.TurnoProfesionales).Id);

            Assert.Equal(
                pacienteHistoricoId,
                Assert.Single(pasado.TurnoPacientes).Id);

            Assert.Equal(
                prestacionHistoricaId,
                Assert.Single(pasado.TurnoTipoPrestaciones).Id);

            #endregion
        }
        finally
        {
            #region Limpieza exclusiva de esta prueba

            if (tipoPruebaId > 0)
            {
                await using var limpieza =
                    BaseDePruebas.CrearContexto();

                var ids = await limpieza.Turnos
                    .Where(t => t.TipoTurnoId == tipoPruebaId)
                    .Select(t => t.Id)
                    .ToListAsync();

                await limpieza.Set<TurnoTipoPrestacion>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Set<TurnoPaciente>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Set<TurnoProfesional>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Turnos
                    .Where(t => t.TipoTurnoId == tipoPruebaId)
                    .ExecuteDeleteAsync();

                if (seriePruebaId > 0)
                {
                    await limpieza.Set<SerieTurno>()
                        .Where(s => s.Id == seriePruebaId)
                        .ExecuteDeleteAsync();
                }

                await limpieza.Set<TipoTurno>()
                    .Where(t => t.Id == tipoPruebaId)
                    .ExecuteDeleteAsync();
            }

            #endregion
        }
    }

    #endregion
}
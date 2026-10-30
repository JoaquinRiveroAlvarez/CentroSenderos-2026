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

public class ObraSocialHistoricaEdicionTests
{
    #region Histórico durante la edición

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Editar_ConservaObraSocialHistorica(
        bool modificarTodaLaSerie)
    {
        var tipoPruebaId = 0;
        var seriePruebaId = 0;
        var obraPruebaId = 0;

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

            var paciente = await contexto.Pacientes
                .Where(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => new
                {
                    p.Id,
                    p.TipoObraSocialId
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(paciente);

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(consultorioId > 0);

            #endregion

            #region Preparación del período

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var libre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var hasta = desde.AddDays(8);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    libre = true;
                    break;
                }

                fecha = fecha.AddDays(8);
            }

            Assert.True(libre);

            #endregion

            #region Obra social histórica y tipo exclusivo

            var historica = new TipoObraSocial
            {
                Tipo = $"TEST-HIST-{Guid.NewGuid():N}",
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(historica);
            await contexto.SaveChangesAsync();
            obraPruebaId = historica.Id;

            Assert.NotEqual(
                paciente.TipoObraSocialId,
                historica.Id);

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-EDIT-HIST-{Guid.NewGuid():N}",
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
                    fecha, DateTimeKind.Utc),
                FechaHasta = DateTime.SpecifyKind(
                    fecha.AddDays(7), DateTimeKind.Utc),
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(serie);
            await contexto.SaveChangesAsync();
            seriePruebaId = serie.Id;

            #endregion

            #region Turnos con histórico distinto de la ficha actual

            var turnos = new List<Turno>();

            for (var i = 0; i < 2; i++)
            {
                var inicio = DateTime.SpecifyKind(
                    fecha.AddDays(i * 7).AddHours(10),
                    DateTimeKind.Utc);

                turnos.Add(new Turno
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
                            PacienteId = paciente.Id,
                            TipoObraSocialId = historica.Id,
                            NombreObraSocial = historica.Tipo,
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

            contexto.AddRange(turnos);
            await contexto.SaveChangesAsync();

            #endregion

            #region Edición

            var dto = new TurnoDTO
            {
                Id = turnos[0].Id,
                Fecha = fecha,
                Hora = new TimeOnly(12, 0),
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioId,
                EstadoTurno = EnumEstadoTurno.reservado,
                SerieTurnoId = serie.Id,
                ModificarTodaLaSerie = modificarTodaLaSerie,
                FrecuenciaRecurrencia =
                    EnumFrecuenciaRecurrenciaTurno.semanal,
                FechaHastaRecurrencia = fecha.AddDays(7),
                ProfesionalIds = new List<int>
                {
                    relacion.ProfesionalId
                },
                PacienteIds = new List<int>
                {
                    paciente.Id
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
                turnos[0].Id, dto));

            #endregion

            #region Verificación independiente

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var guardados = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoPacientes)
                .Where(t => t.TipoTurnoId == tipoPruebaId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            Assert.Equal(2, guardados.Count);

            for (var i = 0; i < 2; i++)
            {
                var debeCambiar = modificarTodaLaSerie || i == 0;
                var horaEsperada = debeCambiar ? 12 : 10;

                var inicioEsperado = DateTime.SpecifyKind(
                    fecha.AddDays(i * 7).AddHours(horaEsperada),
                    DateTimeKind.Utc);

                Assert.Equal(
                    inicioEsperado,
                    guardados[i].FechaInicio);

                var relacionPaciente = Assert.Single(
                    guardados[i].TurnoPacientes);

                Assert.Equal(
                    paciente.Id,
                    relacionPaciente.PacienteId);

                Assert.Equal(
                    historica.Id,
                    relacionPaciente.TipoObraSocialId);

                Assert.Equal(
                    historica.Tipo,
                    relacionPaciente.NombreObraSocial);
            }

            var obraActual = await verificacion.Pacientes
                .Where(p => p.Id == paciente.Id)
                .Select(p => p.TipoObraSocialId)
                .SingleAsync();

            Assert.Equal(paciente.TipoObraSocialId, obraActual);

            #endregion
        }
        finally
        {
            #region Limpieza de datos propios

            await using var limpieza =
                BaseDePruebas.CrearContexto();

            if (tipoPruebaId > 0)
            {
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
            }

            if (seriePruebaId > 0)
            {
                await limpieza.Set<SerieTurno>()
                    .Where(s => s.Id == seriePruebaId)
                    .ExecuteDeleteAsync();
            }

            if (tipoPruebaId > 0)
            {
                await limpieza.Set<TipoTurno>()
                    .Where(t => t.Id == tipoPruebaId)
                    .ExecuteDeleteAsync();
            }

            if (obraPruebaId > 0)
            {
                await limpieza.Set<TipoObraSocial>()
                    .Where(o => o.Id == obraPruebaId)
                    .ExecuteDeleteAsync();
            }

            #endregion
        }
    }

    #endregion
}
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

public class SeriesTurnosPersistenciaTests
{
    #region Persistencia de series

    [Theory]
    [InlineData(EnumFrecuenciaRecurrenciaTurno.semanal, null)]
    [InlineData(EnumFrecuenciaRecurrenciaTurno.cadaQuinceDias, null)]
    [InlineData(EnumFrecuenciaRecurrenciaTurno.mensual, null)]
    [InlineData(
        EnumFrecuenciaRecurrenciaTurno.personalizado,
        EnumUnidadRecurrenciaTurno.dias)]
    [InlineData(
        EnumFrecuenciaRecurrenciaTurno.personalizado,
        EnumUnidadRecurrenciaTurno.semanas)]
    [InlineData(
        EnumFrecuenciaRecurrenciaTurno.personalizado,
        EnumUnidadRecurrenciaTurno.meses)]
    public async Task AltaSerie_GuardaTresOcurrenciasYFechaLimite(
        EnumFrecuenciaRecurrenciaTurno frecuencia,
        EnumUnidadRecurrenciaTurno? unidad)
    {
        var tipoPruebaId = 0;

        try
        {
            await using var contexto = BaseDePruebas.CrearContexto();

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

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                consultorioId > 0,
                "Se necesita un consultorio activo en testing.");

            #endregion

            #region Fechas esperadas

            // El día 2 evita ajustes de fin de mes.
            var fecha = new DateTime(
                DateTime.UtcNow.Year + 2, 1, 2);

            DateTime[] esperadas = Array.Empty<DateTime>();
            var periodoLibre = false;

            for (var intento = 0; intento < 36; intento++)
            {
                esperadas = frecuencia switch
                {
                    EnumFrecuenciaRecurrenciaTurno.semanal =>
                        new[]
                        {
                            fecha,
                            fecha.AddDays(7),
                            fecha.AddDays(14)
                        },

                    EnumFrecuenciaRecurrenciaTurno.cadaQuinceDias =>
                        new[]
                        {
                            fecha,
                            fecha.AddDays(15),
                            fecha.AddDays(30)
                        },

                    EnumFrecuenciaRecurrenciaTurno.mensual =>
                        new[]
                        {
                            fecha,
                            fecha.AddMonths(1),
                            fecha.AddMonths(2)
                        },

                    EnumFrecuenciaRecurrenciaTurno.personalizado =>
                        unidad switch
                        {
                            EnumUnidadRecurrenciaTurno.dias =>
                                new[]
                                {
                                    fecha,
                                    fecha.AddDays(2),
                                    fecha.AddDays(4)
                                },

                            EnumUnidadRecurrenciaTurno.semanas =>
                                new[]
                                {
                                    fecha,
                                    fecha.AddDays(14),
                                    fecha.AddDays(28)
                                },

                            EnumUnidadRecurrenciaTurno.meses =>
                                new[]
                                {
                                    fecha,
                                    fecha.AddMonths(2),
                                    fecha.AddMonths(4)
                                },

                            _ => throw new InvalidOperationException(
                                "Falta una unidad personalizada válida.")
                        },

                    _ => throw new InvalidOperationException(
                        "Frecuencia no prevista por esta prueba.")
                };

                var desde = DateTime.SpecifyKind(
                    esperadas[0], DateTimeKind.Utc);

                var hasta = DateTime.SpecifyKind(
                    esperadas[2].AddDays(1),
                    DateTimeKind.Utc);

                var ocupado = await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde);

                if (!ocupado)
                {
                    periodoLibre = true;
                    break;
                }

                fecha = fecha.AddMonths(3);
            }

            Assert.True(
                periodoLibre,
                "No se encontró un período libre para la serie.");

            #endregion

            #region Creación de la serie

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-SERIE-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            tipoPruebaId = tipo.Id;

            var intervalo = unidad.HasValue ? 2 : 1;

            var dto = new TurnoDTO
            {
                Fecha = esperadas[0],
                Hora = new TimeOnly(10, 0),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioId,
                FrecuenciaRecurrencia = frecuencia,
                FechaHastaRecurrencia = esperadas[2],
                IntervaloRecurrencia = intervalo,
                UnidadRecurrencia = unidad,
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

            var primerId = await repositorio.InsertarTurno(dto);

            Assert.True(primerId > 0);

            #endregion

            #region Verificación desde otro contexto

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var turnos = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.SerieTurno)
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .Where(t => t.TipoTurnoId == tipoPruebaId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            Assert.Equal(3, turnos.Count);
            Assert.Equal(primerId, turnos[0].Id);

            var serieId = turnos[0].SerieTurnoId;

            Assert.True(serieId.HasValue);

            for (var i = 0; i < esperadas.Length; i++)
            {
                var turno = turnos[i];

                var inicio = DateTime.SpecifyKind(
                    esperadas[i].AddHours(10),
                    DateTimeKind.Utc);

                Assert.Equal(inicio, turno.FechaInicio);

                Assert.Equal(
                    inicio.AddMinutes(30),
                    turno.FechaFin);

                Assert.Equal(serieId, turno.SerieTurnoId);
                Assert.Equal(tipo.Id, turno.TipoTurnoId);

                Assert.Equal(
                    consultorioId,
                    turno.TipoConsultorioId);

                Assert.Equal(
                    EnumEstadoTurno.reservado,
                    turno.EstadoTurno);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    turno.EstadoRegistro);

                var profesional = Assert.Single(
                    turno.TurnoProfesionales);

                Assert.Equal(
                    relacion.ProfesionalId,
                    profesional.ProfesionalId);

                var paciente = Assert.Single(
                    turno.TurnoPacientes);

                Assert.Equal(
                    pacienteId,
                    paciente.PacienteId);

                var prestacion = Assert.Single(
                    turno.TurnoTipoPrestaciones);

                Assert.Equal(
                    relacion.ProfesionalId,
                    prestacion.ProfesionalId);

                Assert.Equal(
                    relacion.TipoPrestacionId,
                    prestacion.TipoPrestacionId);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    prestacion.EstadoRegistro);
            }

            var serie = turnos[0].SerieTurno;

            Assert.NotNull(serie);

            Assert.Equal(frecuencia, serie.Frecuencia);
            Assert.Equal(intervalo, serie.Intervalo);
            Assert.Equal(unidad, serie.UnidadPersonalizada);

            Assert.Equal(
                esperadas[0],
                serie.FechaInicio.Date);

            Assert.Equal(
                esperadas[2],
                serie.FechaHasta.Date);

            #endregion
        }
        finally
        {
            #region Limpieza exclusiva de esta prueba

            if (tipoPruebaId > 0)
            {
                await using var limpieza =
                    BaseDePruebas.CrearContexto();

                var propios = await limpieza.Turnos
                    .Where(t => t.TipoTurnoId == tipoPruebaId)
                    .Select(t => new
                    {
                        t.Id,
                        t.SerieTurnoId
                    })
                    .ToListAsync();

                var ids = propios
                    .Select(t => t.Id)
                    .ToList();

                var seriesPropias = propios
                    .Where(t => t.SerieTurnoId.HasValue)
                    .Select(t => t.SerieTurnoId!.Value)
                    .Distinct()
                    .ToList();

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

                await limpieza.Set<SerieTurno>()
                    .Where(s => seriesPropias.Contains(s.Id))
                    .ExecuteDeleteAsync();

                await limpieza.Set<TipoTurno>()
                    .Where(t => t.Id == tipoPruebaId)
                    .ExecuteDeleteAsync();
            }

            #endregion
        }
    }

    #endregion
}
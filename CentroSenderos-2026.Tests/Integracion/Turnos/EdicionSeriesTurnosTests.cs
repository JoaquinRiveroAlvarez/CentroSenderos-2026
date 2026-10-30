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

public class EdicionSeriesTurnosTests
{
    #region Edición individual y de serie futura

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EditarSerie_ActualizaSoloLasOcurrenciasIndicadas(
        bool modificarTodaLaSerie)
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

            Assert.True(pacienteId > 0);

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(consultorioId > 0);

            #endregion

            #region Preparación de tres turnos futuros

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

            Assert.True(periodoLibre);

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-EDIT-SERIE-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();
            tipoPruebaId = tipo.Id;

            var dto = new TurnoDTO
            {
                Fecha = fecha,
                Hora = new TimeOnly(10, 0),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioId,
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
            await repositorio.InsertarTurno(dto);

            var originales = await contexto.Turnos
                .AsNoTracking()
                .Where(t => t.TipoTurnoId == tipoPruebaId)
                .OrderBy(t => t.FechaInicio)
                .Select(t => new
                {
                    t.Id,
                    t.FechaInicio,
                    t.SerieTurnoId
                })
                .ToListAsync();

            Assert.Equal(3, originales.Count);
            Assert.True(originales[1].SerieTurnoId.HasValue);

            #endregion

            #region Edición desde la segunda ocurrencia

            var seleccionado = originales[1];

            dto.Id = seleccionado.Id;
            dto.Fecha = fecha.AddDays(7);
            dto.Hora = new TimeOnly(12, 0);
            dto.SerieTurnoId = seleccionado.SerieTurnoId;
            dto.ModificarTodaLaSerie = modificarTodaLaSerie;

            var actualizado = await repositorio.ActualizarTurno(
                seleccionado.Id, dto);

            Assert.True(actualizado);

            #endregion

            #region Verificación desde otro contexto

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var posteriores = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .Where(t => t.TipoTurnoId == tipoPruebaId)
                .ToListAsync();

            Assert.Equal(3, posteriores.Count);

            foreach (var original in originales)
            {
                var turno = Assert.Single(
                    posteriores.Where(t => t.Id == original.Id));

                // Toda la serie significa todas las ocurrencias
                // futuras respecto de hoy, incluida la primera.
                var debeCambiar =
                    modificarTodaLaSerie ||
                    original.Id == seleccionado.Id;

                var inicioEsperado = debeCambiar
                    ? original.FechaInicio.AddHours(2)
                    : original.FechaInicio;

                Assert.Equal(
                    inicioEsperado,
                    turno.FechaInicio);

                Assert.Equal(
                    inicioEsperado.AddMinutes(30),
                    turno.FechaFin);

                Assert.Equal(
                    original.SerieTurnoId,
                    turno.SerieTurnoId);

                Assert.Equal(
                    consultorioId,
                    turno.TipoConsultorioId);

                Assert.Equal(
                    EnumEstadoTurno.reservado,
                    turno.EstadoTurno);

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

                var ids = propios.Select(t => t.Id).ToList();

                var series = propios
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
                    .Where(s => series.Contains(s.Id))
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
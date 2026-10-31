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

public class TurnosSeleccionMultipleTests
{
    #region Alta y edición con múltiples participantes

    [Fact]
    public async Task AltaYEdicion_ConservanParticipantesYPrestacionPorProfesional()
    {
        var tipoPruebaId = 0;

        try
        {
            await using var contexto = BaseDePruebas.CrearContexto();

            #region Referencias

            var relaciones = await contexto.ProfesionalTipoPrestaciones
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
                .ToListAsync();

            var selecciones = relaciones
                .GroupBy(r => r.ProfesionalId)
                .OrderBy(g => g.Key)
                .Take(2)
                .Select(g => g.OrderBy(r => r.TipoPrestacionId).First())
                .ToList();

            Assert.True(
                selecciones.Count == 2,
                "Se necesitan dos profesionales activos con prestaciones activas.");

            var pacientes = await contexto.Pacientes
                .Where(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .Take(2)
                .ToListAsync();

            Assert.True(
                pacientes.Count == 2,
                "Se necesitan dos pacientes activos en testing.");

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(consultorioId > 0);

            #endregion

            #region Preparación

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var libre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var hasta = desde.AddDays(1);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    libre = true;
                    break;
                }

                fecha = fecha.AddDays(1);
            }

            Assert.True(libre);

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-MULTIPLE-{Guid.NewGuid():N}",
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
                ProfesionalIds = selecciones
                    .Select(r => r.ProfesionalId)
                    .ToList(),
                PacienteIds = pacientes.ToList(),
                ProfesionalPrestaciones = selecciones
                    .Select(r => new TurnoProfesionalPrestacionDTO
                    {
                        ProfesionalId = r.ProfesionalId,
                        TipoPrestacionId = r.TipoPrestacionId
                    })
                    .ToList()
            };

            var repositorio = new TurnoRepositorio(contexto);

            #endregion

            #region Alta y consulta

            var id = await repositorio.InsertarTurno(dto);
            Assert.True(id > 0);

            await using (var lectura = BaseDePruebas.CrearContexto())
            {
                var consultas = new TurnoRepositorio(lectura);
                var detalle = await consultas.SelectPorId(id);

                Assert.NotNull(detalle);

                Assert.Equal(
                    dto.ProfesionalIds.OrderBy(x => x).ToArray(),
                    detalle.ProfesionalIds.OrderBy(x => x).ToArray());

                Assert.Equal(
                    pacientes.OrderBy(x => x).ToArray(),
                    detalle.PacienteIds.OrderBy(x => x).ToArray());

                Assert.Equal(2, detalle.ProfesionalPrestaciones.Count);

                foreach (var seleccion in selecciones)
                {
                    var prestacion = Assert.Single(
                        detalle.ProfesionalPrestaciones.Where(r =>
                            r.ProfesionalId == seleccion.ProfesionalId));

                    Assert.Equal(
                        seleccion.TipoPrestacionId,
                        prestacion.TipoPrestacionId);
                }

                var listado = await consultas.SelectListaTurnos();

                var fila = Assert.Single(
                    listado.Where(t => t.Id == id));

                Assert.Equal(
                    dto.ProfesionalIds.OrderBy(x => x).ToArray(),
                    fila.ProfesionalIds.OrderBy(x => x).ToArray());

                Assert.Equal(
                    pacientes.OrderBy(x => x).ToArray(),
                    fila.PacienteIds.OrderBy(x => x).ToArray());

                Assert.Equal(2, fila.ProfesionalPrestaciones.Count);

                foreach (var seleccion in selecciones)
                {
                    var prestacion = Assert.Single(
                        fila.ProfesionalPrestaciones.Where(r =>
                            r.ProfesionalId == seleccion.ProfesionalId));

                    Assert.Equal(
                        seleccion.TipoPrestacionId,
                        prestacion.TipoPrestacionId);
                }
            }

            #endregion

            #region Edición para conservar un participante de cada tipo

            var conservado = selecciones[1];

            dto.Id = id;
            dto.Hora = new TimeOnly(12, 0);

            dto.ProfesionalIds = new List<int>
            {
                conservado.ProfesionalId
            };

            dto.PacienteIds = new List<int>
            {
                pacientes[1]
            };

            dto.ProfesionalPrestaciones =
                new List<TurnoProfesionalPrestacionDTO>
                {
                    new()
                    {
                        ProfesionalId = conservado.ProfesionalId,
                        TipoPrestacionId = conservado.TipoPrestacionId
                    }
                };

            Assert.True(await repositorio.ActualizarTurno(id, dto));

            #endregion

            #region Verificación de las relaciones reemplazadas

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var editado = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .SingleAsync(t => t.Id == id);

            Assert.Equal(
                conservado.ProfesionalId,
                Assert.Single(
                    editado.TurnoProfesionales).ProfesionalId);

            Assert.Equal(
                pacientes[1],
                Assert.Single(
                    editado.TurnoPacientes).PacienteId);

            var prestacionFinal = Assert.Single(
                editado.TurnoTipoPrestaciones);

            Assert.Equal(
                conservado.ProfesionalId,
                prestacionFinal.ProfesionalId);

            Assert.Equal(
                conservado.TipoPrestacionId,
                prestacionFinal.TipoPrestacionId);

            Assert.Equal(
                DateTime.SpecifyKind(
                    fecha.AddHours(12), DateTimeKind.Utc),
                editado.FechaInicio);

            Assert.Equal(
                1,
                await verificacion.Turnos.CountAsync(t =>
                    t.TipoTurnoId == tipoPruebaId));

            #endregion
        }
        finally
        {
            #region Limpieza de registros propios

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

                await limpieza.Set<TipoTurno>()
                    .Where(t => t.Id == tipoPruebaId)
                    .ExecuteDeleteAsync();
            }

            #endregion
        }
    }

    #endregion
}
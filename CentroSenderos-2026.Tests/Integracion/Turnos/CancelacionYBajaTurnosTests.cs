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

public class CancelacionYBajaTurnosTests
{
    #region Cancelación y baja lógica

    [Theory]
    [InlineData("cancelar")]
    [InlineData("eliminar")]
    public async Task CancelarOEliminar_ConservaRegistroYLiberaRecursos(
        string operacion)
    {
        var tipoPruebaId = 0;

        try
        {
            await using var contexto = BaseDePruebas.CrearContexto();

            #region Preparación

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

            Assert.True(
                consultorios.Count == 2,
                "Se necesitan dos consultorios activos en testing.");

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var fechaLibre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var inicioDia = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var finDia = inicioDia.AddDays(1);

                if (!await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < finDia &&
                    t.FechaFin > inicioDia))
                {
                    fechaLibre = true;
                    break;
                }

                fecha = fecha.AddDays(1);
            }

            Assert.True(fechaLibre);

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-BAJA-{Guid.NewGuid():N}",
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
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorios[0],
                EstadoTurno = EnumEstadoTurno.reservado,
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
            var id = await repositorio.InsertarTurno(dto);

            #endregion

            #region Comprobación del bloqueo inicial

            // Consultamos cada recurso por separado.
            for (var recurso = 0; recurso < 3; recurso++)
            {
                var horarios = await repositorio.HorariosDisponibles(
                    DateOnly.FromDateTime(fecha),
                    tipo.Id,
                    recurso == 0 ? consultorios[0] : consultorios[1],
                    recurso == 1
                        ? new List<int> { relacion.ProfesionalId }
                        : new List<int>(),
                    recurso == 2
                        ? new List<int> { pacienteId }
                        : new List<int>());

                Assert.DoesNotContain(new TimeOnly(10, 0), horarios);
            }

            #endregion

            #region Operación

            bool resultado;

            if (operacion == "cancelar")
            {
                dto.Id = id;
                dto.EstadoTurno = EnumEstadoTurno.cancelado;

                resultado = await repositorio.ActualizarTurno(id, dto);
            }
            else
            {
                resultado = await repositorio.DeleteTurno(id);
            }

            Assert.True(resultado);

            #endregion

            #region Persistencia y listado

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var guardado = await verificacion.Turnos
                .AsNoTracking()
                .SingleAsync(t => t.Id == id);

            var inicioEsperado = DateTime.SpecifyKind(
                fecha.AddHours(10), DateTimeKind.Utc);

            Assert.Equal(inicioEsperado, guardado.FechaInicio);
            Assert.Equal(
                inicioEsperado.AddMinutes(30),
                guardado.FechaFin);

            var repositorioVerificacion =
                new TurnoRepositorio(verificacion);

            var listado =
                await repositorioVerificacion.SelectListaTurnos();

            if (operacion == "cancelar")
            {
                Assert.Equal(
                    EnumEstadoTurno.cancelado,
                    guardado.EstadoTurno);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    guardado.EstadoRegistro);

                Assert.Contains(listado, t => t.Id == id);
            }
            else
            {
                Assert.Equal(
                    EnumEstadoRegistro.inactivo,
                    guardado.EstadoRegistro);

                Assert.DoesNotContain(listado, t => t.Id == id);
            }

            #endregion

            #region Liberación de los tres recursos

            for (var recurso = 0; recurso < 3; recurso++)
            {
                var horarios =
                    await repositorioVerificacion.HorariosDisponibles(
                        DateOnly.FromDateTime(fecha),
                        tipo.Id,
                        recurso == 0
                            ? consultorios[0]
                            : consultorios[1],
                        recurso == 1
                            ? new List<int> { relacion.ProfesionalId }
                            : new List<int>(),
                        recurso == 2
                            ? new List<int> { pacienteId }
                            : new List<int>());

                Assert.Contains(new TimeOnly(10, 0), horarios);
            }

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
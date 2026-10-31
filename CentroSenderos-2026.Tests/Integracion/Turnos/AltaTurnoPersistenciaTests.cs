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

public class AltaTurnoPersistenciaTests
{
    #region Circuito de alta y edición

    [Fact]
    public async Task AltaYEdicion_GuardanHorarioParticipantesYPrestacion()
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

            var paciente = await contexto.Pacientes
                .Where(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => new
                {
                    p.Id,
                    p.TipoObraSocialId,
                    NombreObraSocial = p.TipoObraSociales != null
                        ? p.TipoObraSociales.Tipo
                        : null
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(paciente);

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                consultorioId > 0,
                "Se necesita un consultorio activo en testing.");

            #endregion

            #region Preparación

            // Buscamos dos días consecutivos libres:
            // uno para el alta y otro para la edición.
            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var fechasLibres = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var inicioPeriodo = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var finPeriodo = inicioPeriodo.AddDays(2);

                var ocupado = await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < finPeriodo &&
                    t.FechaFin > inicioPeriodo);

                if (!ocupado)
                {
                    fechasLibres = true;
                    break;
                }

                fecha = fecha.AddDays(1);
            }

            Assert.True(
                fechasLibres,
                "No se encontraron dos días libres para la prueba.");

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-PERSIST-{Guid.NewGuid():N}",
                DuracionMinutos = 45,
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

            #endregion

            #region Alta

            var id = await repositorio.InsertarTurno(dto);

            Assert.True(id > 0);

            #endregion

            #region Verificación del alta

            await using (var verificacion =
                BaseDePruebas.CrearContexto())
            {
                var guardado = await verificacion.Turnos
                    .AsNoTracking()
                    .Include(t => t.TurnoProfesionales)
                    .Include(t => t.TurnoPacientes)
                    .Include(t => t.TurnoTipoPrestaciones)
                    .SingleAsync(t => t.Id == id);

                var inicioEsperado = DateTime.SpecifyKind(
                    fecha.AddHours(10),
                    DateTimeKind.Utc);

                Assert.Equal(
                    inicioEsperado,
                    guardado.FechaInicio);

                Assert.Equal(
                    inicioEsperado.AddMinutes(45),
                    guardado.FechaFin);

                Assert.Equal(
                    EnumEstadoTurno.reservado,
                    guardado.EstadoTurno);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    guardado.EstadoRegistro);

                Assert.Equal(
                    tipo.Id,
                    guardado.TipoTurnoId);

                Assert.Equal(
                    consultorioId,
                    guardado.TipoConsultorioId);

                Assert.Null(guardado.SerieTurnoId);

                var profesional = Assert.Single(
                    guardado.TurnoProfesionales);

                Assert.Equal(
                    relacion.ProfesionalId,
                    profesional.ProfesionalId);

                var pacienteGuardado = Assert.Single(
                    guardado.TurnoPacientes);

                Assert.Equal(
                    paciente.Id,
                    pacienteGuardado.PacienteId);

                Assert.Equal(
                    paciente.TipoObraSocialId,
                    pacienteGuardado.TipoObraSocialId);

                Assert.Equal(
                    paciente.NombreObraSocial,
                    pacienteGuardado.NombreObraSocial);

                var prestacion = Assert.Single(
                    guardado.TurnoTipoPrestaciones);

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

            #endregion

            #region Edición individual

            dto.Id = id;
            dto.Fecha = fecha.AddDays(1);
            dto.Hora = new TimeOnly(12, 0);

            var actualizado = await repositorio.ActualizarTurno(
                id, dto);

            Assert.True(actualizado);

            #endregion

            #region Verificación de la edición

            await using (var verificacionEdicion =
                BaseDePruebas.CrearContexto())
            {
                var editado = await verificacionEdicion.Turnos
                    .AsNoTracking()
                    .Include(t => t.TurnoProfesionales)
                    .Include(t => t.TurnoPacientes)
                    .Include(t => t.TurnoTipoPrestaciones)
                    .SingleAsync(t => t.Id == id);

                var nuevoInicio = DateTime.SpecifyKind(
                    dto.Fecha.Date.AddHours(12),
                    DateTimeKind.Utc);

                Assert.Equal(
                    nuevoInicio,
                    editado.FechaInicio);

                Assert.Equal(
                    nuevoInicio.AddMinutes(45),
                    editado.FechaFin);

                Assert.Equal(
                    EnumEstadoTurno.reservado,
                    editado.EstadoTurno);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    editado.EstadoRegistro);

                Assert.Equal(
                    consultorioId,
                    editado.TipoConsultorioId);

                Assert.Equal(
                    tipo.Id,
                    editado.TipoTurnoId);

                Assert.Null(editado.SerieTurnoId);

                var profesionalEditado = Assert.Single(
                    editado.TurnoProfesionales);

                Assert.Equal(
                    relacion.ProfesionalId,
                    profesionalEditado.ProfesionalId);

                var pacienteEditado = Assert.Single(
                    editado.TurnoPacientes);

                Assert.Equal(
                    paciente.Id,
                    pacienteEditado.PacienteId);

                Assert.Equal(
                    paciente.TipoObraSocialId,
                    pacienteEditado.TipoObraSocialId);

                Assert.Equal(
                    paciente.NombreObraSocial,
                    pacienteEditado.NombreObraSocial);

                var prestacionEditada = Assert.Single(
                    editado.TurnoTipoPrestaciones);

                Assert.Equal(
                    relacion.ProfesionalId,
                    prestacionEditada.ProfesionalId);

                Assert.Equal(
                    relacion.TipoPrestacionId,
                    prestacionEditada.TipoPrestacionId);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    prestacionEditada.EstadoRegistro);

                // Editar debe actualizar el turno existente,
                // sin crear un segundo registro.
                var cantidad = await verificacionEdicion.Turnos
                    .CountAsync(t =>
                        t.TipoTurnoId == tipoPruebaId);

                Assert.Equal(1, cantidad);
            }

            #endregion
        }
        finally
        {
            #region Limpieza de los datos propios de la prueba

            if (tipoPruebaId > 0)
            {
                await using var limpieza =
                    BaseDePruebas.CrearContexto();

                var ids = await limpieza.Turnos
                    .Where(t =>
                        t.TipoTurnoId == tipoPruebaId)
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
                    .Where(t =>
                        t.TipoTurnoId == tipoPruebaId)
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
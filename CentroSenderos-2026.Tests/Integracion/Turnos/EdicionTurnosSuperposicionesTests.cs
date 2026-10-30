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

public class EdicionTurnosSuperposicionesTests
{
    #region Rechazo de conflictos al editar

    // Turno ocupado: 10:00 a 11:00.
    // Turno que editamos: originalmente 12:00 a 13:00.
    // Intentamos moverlo a 10:00 o 10:30.
    [Theory]
    [InlineData("consultorio", 0)]
    [InlineData("consultorio", 30)]
    [InlineData("profesional", 0)]
    [InlineData("profesional", 30)]
    [InlineData("paciente", 0)]
    [InlineData("paciente", 30)]
    public async Task Edicion_RecursoOcupado_RechazaYConservaElTurno(
        string recurso,
        int desplazamiento)
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

            #region Preparación

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

            Assert.True(
                fechaLibre,
                "No se encontró un día libre para la prueba.");

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-EDIT-{Guid.NewGuid():N}",
                DuracionMinutos = 60,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            var inicioOcupado = DateTime.SpecifyKind(
                fecha.AddHours(10), DateTimeKind.Utc);

            var ocupado = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorios[0],
                FechaInicio = inicioOcupado,
                FechaFin = inicioOcupado.AddHours(1),
                EstadoTurno = EnumEstadoTurno.reservado,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            // Aislamos el recurso que debe provocar el rechazo.
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

            var consultorioEdicion = recurso == "consultorio"
                ? consultorios[0]
                : consultorios[1];

            var inicioOriginal = DateTime.SpecifyKind(
                fecha.AddHours(12), DateTimeKind.Utc);

            var editable = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioEdicion,
                FechaInicio = inicioOriginal,
                FechaFin = inicioOriginal.AddHours(1),
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
                TurnoTipoPrestaciones = new List<TurnoTipoPrestacion>
                {
                    new()
                    {
                        ProfesionalId = relacion.ProfesionalId,
                        TipoPrestacionId = relacion.TipoPrestacionId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }
                }
            };

            contexto.AddRange(ocupado, editable);
            await contexto.SaveChangesAsync();

            #endregion

            #region Intento de edición

            var dto = new TurnoDTO
            {
                Id = editable.Id,
                Fecha = fecha,
                Hora = new TimeOnly(10, 0).AddMinutes(desplazamiento),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioEdicion,
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
                    editable.Id, dto));

            Assert.Contains(
                recurso,
                error.Message.ToLowerInvariant());

            #endregion

            #region Verificación de los datos conservados

            // AsNoTracking obliga a leer lo guardado en la base.
            var conservado = await contexto.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .SingleAsync(t => t.Id == editable.Id);

            Assert.Equal(inicioOriginal, conservado.FechaInicio);
            Assert.Equal(
                inicioOriginal.AddHours(1),
                conservado.FechaFin);

            Assert.Equal(
                consultorioEdicion,
                conservado.TipoConsultorioId);

            Assert.Equal(tipo.Id, conservado.TipoTurnoId);
            Assert.Equal(
                EnumEstadoTurno.reservado,
                conservado.EstadoTurno);

            Assert.Equal(
                relacion.ProfesionalId,
                Assert.Single(
                    conservado.TurnoProfesionales).ProfesionalId);

            Assert.Equal(
                pacienteId,
                Assert.Single(
                    conservado.TurnoPacientes).PacienteId);

            var prestacion = Assert.Single(
                conservado.TurnoTipoPrestaciones);

            Assert.Equal(
                relacion.ProfesionalId,
                prestacion.ProfesionalId);

            Assert.Equal(
                relacion.TipoPrestacionId,
                prestacion.TipoPrestacionId);

            Assert.Equal(
                2,
                await contexto.Turnos.CountAsync(t =>
                    t.TipoTurnoId == tipo.Id));

            #endregion
        }
        finally
        {
            // Revierte todos los datos preparados por este caso.
            await transaccion.RollbackAsync();
        }
    }

    #endregion
}
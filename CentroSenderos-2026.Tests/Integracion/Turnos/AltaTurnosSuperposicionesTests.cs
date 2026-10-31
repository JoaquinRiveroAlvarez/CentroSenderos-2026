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

public class AltaTurnosSuperposicionesTests
{
    #region Rechazo de superposiciones al guardar

    // Turno existente: 10:00 a 11:00.
    // Turno solicitado: 10:00 a 11:00 o 10:30 a 11:30.
    [Theory]
    [InlineData("consultorio", 0)]
    [InlineData("consultorio", 30)]
    [InlineData("profesional", 0)]
    [InlineData("profesional", 30)]
    [InlineData("paciente", 0)]
    [InlineData("paciente", 30)]
    public async Task Alta_RecursoOcupado_RechazaSinGuardar(
        string recurso,
        int minutosDesplazamiento)
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
                "Se necesita un paciente activo en la base de testing.");

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

            #region Preparación del turno ocupado

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-ALTA-{Guid.NewGuid():N}",
                DuracionMinutos = 60,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var fechaLibre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var inicioDia = DateTime.SpecifyKind(
                    fecha, DateTimeKind.Utc);

                var finDia = inicioDia.AddDays(1);

                var ocupado = await contexto.Turnos.AnyAsync(t =>
                    t.FechaInicio < finDia &&
                    t.FechaFin > inicioDia);

                if (!ocupado)
                {
                    fechaLibre = true;
                    break;
                }

                fecha = fecha.AddDays(1);
            }

            Assert.True(
                fechaLibre,
                "No se encontró un día libre para aislar la prueba.");

            var inicio = DateTime.SpecifyKind(
                fecha.AddHours(10), DateTimeKind.Utc);

            var existente = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorios[0],
                FechaInicio = inicio,
                FechaFin = inicio.AddHours(1),
                EstadoRegistro = EnumEstadoRegistro.activo,
                EstadoTurno = EnumEstadoTurno.reservado
            };

            // Cada escenario bloquea solamente el recurso evaluado.
            if (recurso == "profesional")
            {
                existente.TurnoProfesionales.Add(
                    new TurnoProfesional
                    {
                        ProfesionalId = relacion.ProfesionalId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    });
            }

            if (recurso == "paciente")
            {
                existente.TurnoPacientes.Add(
                    new TurnoPaciente
                    {
                        PacienteId = pacienteId,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    });
            }

            contexto.Add(existente);
            await contexto.SaveChangesAsync();

            #endregion

            #region Solicitud de alta

            var dto = new TurnoDTO
            {
                Fecha = fecha,
                Hora = new TimeOnly(10, 0)
                    .AddMinutes(minutosDesplazamiento),
                EstadoTurno = EnumEstadoTurno.reservado,
                TipoTurnoId = tipo.Id,

                // Para profesional y paciente usamos otro
                // consultorio, evitando un conflicto previo.
                TipoConsultorioId = recurso == "consultorio"
                    ? consultorios[0]
                    : consultorios[1],

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

            #endregion

            #region Resultado y persistencia

            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => await repositorio.InsertarTurno(dto));

            Assert.Contains(
                recurso,
                error.Message.ToLowerInvariant());

            // Debe seguir existiendo solamente el turno preparado.
            var cantidad = await contexto.Turnos
                .AsNoTracking()
                .CountAsync(t => t.TipoTurnoId == tipo.Id);

            Assert.Equal(1, cantidad);

            var guardado = await contexto.Turnos
                .AsNoTracking()
                .SingleAsync(t => t.Id == existente.Id);

            Assert.Equal(inicio, guardado.FechaInicio);
            Assert.Equal(inicio.AddHours(1), guardado.FechaFin);

            #endregion
        }
        finally
        {
            await transaccion.RollbackAsync();
        }
    }

    #endregion
}
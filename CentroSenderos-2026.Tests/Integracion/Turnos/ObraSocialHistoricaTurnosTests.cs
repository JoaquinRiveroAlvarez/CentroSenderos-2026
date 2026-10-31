using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests.Integracion.Turnos;

public class ObraSocialHistoricaTurnosTests
{
    #region Conservación de la obra social histórica

    [Fact]
    public async Task CambiarObraSocialDelPaciente_NoAlteraLaDelTurno()
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        try
        {
            #region Datos de referencia

            var paciente = await contexto.Pacientes
                .Include(p => p.TipoObraSociales)
                .FirstOrDefaultAsync(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo &&
                    p.TipoObraSociales != null);

            Assert.NotNull(paciente);
            Assert.NotNull(paciente.TipoObraSociales);

            var pacienteId = paciente.Id;
            var obraOriginalId = paciente.TipoObraSocialId;
            var nombreOriginal = paciente.TipoObraSociales.Tipo;

            // Creamos la alternativa dentro de la transacción.
            // No dependemos de otra obra social ya cargada.
            var otraObra = new TipoObraSocial
            {
                Tipo = $"TEST-OS-{Guid.NewGuid():N}",
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(otraObra);
            await contexto.SaveChangesAsync();

            var nuevaObraId = otraObra.Id;

            var consultorioId = await contexto.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                consultorioId > 0,
                "Se necesita un consultorio activo en testing.");

            #endregion

            #region Preparación del turno

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-OBRA-HIST-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.Add(tipo);
            await contexto.SaveChangesAsync();

            var inicio = DateTime.SpecifyKind(
                DateTime.UtcNow.Date.AddYears(2).AddHours(10),
                DateTimeKind.Utc);

            var turno = new Turno
            {
                TipoTurnoId = tipo.Id,
                TipoConsultorioId = consultorioId,
                FechaInicio = inicio,
                FechaFin = inicio.AddMinutes(30),
                EstadoTurno = EnumEstadoTurno.reservado,
                EstadoRegistro = EnumEstadoRegistro.activo,
                TurnoPacientes = new List<TurnoPaciente>
                {
                    new()
                    {
                        PacienteId = pacienteId,
                        TipoObraSocialId = obraOriginalId,
                        NombreObraSocial = nombreOriginal,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }
                }
            };

            contexto.Add(turno);
            await contexto.SaveChangesAsync();

            var turnoId = turno.Id;

            #endregion

            #region Cambio en la ficha del paciente

            paciente.TipoObraSocialId = nuevaObraId;
            await contexto.SaveChangesAsync();

            contexto.ChangeTracker.Clear();

            var obraActualId = await contexto.Pacientes
                .AsNoTracking()
                .Where(p => p.Id == pacienteId)
                .Select(p => p.TipoObraSocialId)
                .SingleAsync();

            Assert.Equal(nuevaObraId, obraActualId);

            #endregion

            #region Verificación del detalle

            var repositorio = new TurnoRepositorio(contexto);
            var detalle = await repositorio.SelectPorId(turnoId);

            Assert.NotNull(detalle);

            var pacienteDetalle = Assert.Single(
                detalle.PacientesDetalle);

            Assert.Equal(pacienteId, pacienteDetalle.PacienteId);

            Assert.Equal(
                obraOriginalId,
                pacienteDetalle.TipoObraSocialId);

            Assert.Equal(
                nombreOriginal,
                pacienteDetalle.NombreObraSocial);

            #endregion

            #region Verificación del listado

            var listado = await repositorio.SelectListaTurnos();

            var turnoListado = Assert.Single(
                listado.Where(t => t.Id == turnoId));

            var pacienteListado = Assert.Single(
                turnoListado.PacientesDetalle);

            Assert.Equal(pacienteId, pacienteListado.PacienteId);

            Assert.Equal(
                obraOriginalId,
                pacienteListado.TipoObraSocialId);

            Assert.Equal(
                nombreOriginal,
                pacienteListado.NombreObraSocial);

            #endregion

            #region Verificación de la relación guardada

            var historico = await contexto.Set<TurnoPaciente>()
                .AsNoTracking()
                .SingleAsync(r =>
                    r.TurnoId == turnoId &&
                    r.PacienteId == pacienteId);

            Assert.Equal(
                obraOriginalId,
                historico.TipoObraSocialId);

            Assert.Equal(
                nombreOriginal,
                historico.NombreObraSocial);

            #endregion
        }
        finally
        {
            // Restaura al paciente y revierte el turno,
            // el tipo de turno y la obra social temporal.
            await transaccion.RollbackAsync();
        }
    }

    #endregion
}
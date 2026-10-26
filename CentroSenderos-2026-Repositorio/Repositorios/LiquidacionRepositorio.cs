using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class LiquidacionRepositorio : Repositorio<Liquidacion>, ILiquidacionRepositorio
    {
        private readonly ApplicationDbContext context;

        public LiquidacionRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        // Obtener resumen para la pantalla principal con filtros de fechas y profesional
        public async Task<List<LiquidacionResumenDTO>> ObtenerResumenLiquidaciones(DateTime? fechaDesde, DateTime? fechaHasta, int? profesionalId)
        {
            var query = context.Liquidaciones
                .Include(l => l.Profesionales)
                .Include(l => l.DetalleLiquidaciones)
                    .ThenInclude(d => d.TipoModalidades)
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                var fDesdeUtc = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
                query = query.Where(l => l.FechaHasta >= fDesdeUtc);
            }

            if (fechaHasta.HasValue)
            {
                var fHastaUtc = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(l => l.FechaDesde <= fHastaUtc);
            }

            if (profesionalId.HasValue && profesionalId.Value > 0)
                query = query.Where(l => l.ProfesionalId == profesionalId.Value);

            return await query
                .OrderByDescending(l => l.FechaDesde)
                .Select(l => new LiquidacionResumenDTO
                {
                    Id = l.Id,
                    NumeroLiquidacion = l.NumeroLiquidacion,
                    ProfesionalId = l.ProfesionalId,
                    ProfesionalNombreCompleto = l.Profesionales != null ? l.Profesionales.Nombre : "Sin Asignar",
                    FechaDesde = l.FechaDesde,
                    FechaHasta = l.FechaHasta,
                    TotalFacturacion = l.TotalFacturacion,
                    TotalRetencion = l.TotalRetencion,
                    TotalSaldo = l.TotalSaldo,
                    TotalAbonado = l.TotalRetencion - l.TotalSaldo,
                    CobroProfesional = l.TotalFacturacion - l.TotalRetencion,
                    EstadoPago = l.EstadoPago,
                    CantidadDetalles = l.DetalleLiquidaciones.Count,
                    Modalidades = l.DetalleLiquidaciones
                        .Where(d => d.TipoModalidades != null)
                        .Select(d => d.TipoModalidades!.Tipo)
                        .Distinct()
                        .ToList(),
                    Anulada = l.EstadoRegistro != EnumEstadoRegistro.activo
                })
                .ToListAsync();
        }

        // Obtener detalle completo de cabecera y filas
        public async Task<LiquidacionDTO?> ObtenerPorIdCompleto(int id)
        {
            var l = await context.Liquidaciones
                .Include(x => x.Profesionales)
                .Include(x => x.Socios)
                .Include(x => x.DetalleLiquidaciones)
                    .ThenInclude(d => d.TipoModalidades)
                .Include(x => x.DetalleLiquidaciones)
                    .ThenInclude(d => d.TipoPrestaciones)
                .Include(x => x.DetalleLiquidaciones)
                    .ThenInclude(d => d.TipoObraSociales)
                .Include(x => x.DetalleLiquidaciones)
                    .ThenInclude(d => d.Pacientes)
                .Include(x => x.DetalleLiquidaciones)
                    .ThenInclude(d => d.Turnos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (l == null) return null;

            return new LiquidacionDTO
            {
                Id = l.Id,
                NumeroLiquidacion = l.NumeroLiquidacion,
                FechaDesde = l.FechaDesde,
                FechaHasta = l.FechaHasta,
                ProfesionalId = l.ProfesionalId,
                ProfesionalNombreCompleto = l.Profesionales?.Nombre ?? "",
                SocioId = l.SocioId,
                SocioNombre = l.Socios?.Profesionales?.Nombre ?? "",
                EstadoPago = l.EstadoPago,
                TotalFacturacion = l.TotalFacturacion,
                TotalRetencion = l.TotalRetencion,
                TotalSaldo = l.TotalSaldo,
                Observaciones = l.Observaciones,
                Detalles = l.DetalleLiquidaciones.Select(d => new DetalleLiquidacionDTO
                {
                    Id = d.Id,
                    LiquidacionId = d.LiquidacionId,
                    TipoModalidadId = d.TipoModalidadId,
                    TipoModalidadNombre = d.TipoModalidades?.Tipo ?? "",
                    TipoPrestacionId = d.TipoPrestacionId,
                    TipoPrestacionNombre = d.TipoPrestaciones?.Tipo ?? "",
                    TipoObraSocialId = d.TipoObraSocialId,
                    TipoObraSocialNombre = d.TipoObraSociales?.Tipo ?? "",
                    PacienteId = d.PacienteId,
                    PacienteNombreCompleto = d.Pacientes?.Nombre ?? "Sin Paciente",
                    MontoUnitario = d.MontoUnitario,
                    Cantidad = d.Cantidad,
                    MontoModuloCompleto = d.MontoModuloCompleto,
                    CantidadModuloCompleto = d.CantidadModuloCompleto,
                    MontoMedioModulo = d.MontoMedioModulo,
                    CantidadMedioModulo = d.CantidadMedioModulo,
                    MontoFacturacion = d.MontoFacturacion,
                    Porcentaje = d.Porcentaje,
                    Retencion = d.Retencion,
                    Saldo = d.Saldo,
                    EstadoPago = d.EstadoPago,
                    Observaciones = d.Observaciones,
                    CantidadTurnosAsignados = d.Turnos.Count
                }).ToList()
            };
        }

        public async Task<LiquidacionDTO?> ObtenerPorNumero(string numeroLiquidacion)
        {
            var l = await context.Liquidaciones.FirstOrDefaultAsync(x => x.NumeroLiquidacion == numeroLiquidacion);
            if (l == null) return null;
            return await ObtenerPorIdCompleto(l.Id);
        }

        // Obtener turnos de una fila de detalle
        public async Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPorDetalleId(int detalleLiquidacionId)
        {
            var turnos = await context.Turnos
                .Include(t => t.TipoConsultorios)
                .Include(t => t.TipoTurnos)
                .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                .Where(t => t.DetalleLiquidacionId == detalleLiquidacionId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            var resultado = new List<TurnoLiquidacionItemDTO>();
            int sesionNro = 1;

            foreach (var t in turnos)
            {
                var inicioLocal = t.FechaInicio.ToLocalTime();
                var finLocal = t.FechaFin.ToLocalTime();
                var profRel = t.TurnoProfesionales.FirstOrDefault();
                var pacRel = t.TurnoPacientes.FirstOrDefault();
                var prestRel = t.TurnoTipoPrestaciones.FirstOrDefault();

                resultado.Add(new TurnoLiquidacionItemDTO
                {
                    TurnoId = t.Id,
                    NumeroSesion = sesionNro++,
                    Fecha = inicioLocal.Date,
                    HoraInicio = TimeOnly.FromDateTime(inicioLocal),
                    HoraFin = TimeOnly.FromDateTime(finLocal),
                    ConsultorioNombre = t.TipoConsultorios?.Tipo ?? "Sin Consultorio",
                    TipoTurnoNombre = prestRel?.TipoPrestaciones?.Tipo ?? t.TipoTurnos?.Tipo ?? "Normal",
                    EstadoTurno = t.EstadoTurno,
                    ProfesionalId = profRel?.ProfesionalId,
                    ProfesionalNombre = profRel?.Profesionales?.Nombre ?? "",
                    PacienteId = pacRel?.PacienteId,
                    PacienteNombre = pacRel?.Pacientes?.Nombre ?? "",
                    TipoPrestacionId = prestRel?.TipoPrestacionId,
                    TipoPrestacionNombre = prestRel?.TipoPrestaciones?.Tipo ?? ""
                });
            }

            return resultado;
        }

        // Obtener turnos pendientes sin liquidar
        public async Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPendientes(int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                // Fix UTC para PostgreSQL Neon
                var desdeBase = (fechaDesde ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
                var hastaBase = (fechaHasta ?? desdeBase.AddMonths(1).AddDays(-1)).Date.AddDays(1).AddTicks(-1);

                var desde = DateTime.SpecifyKind(desdeBase, DateTimeKind.Utc);
                var hasta = DateTime.SpecifyKind(hastaBase, DateTimeKind.Utc);

                var query = context.Turnos
                    .AsNoTracking()
                    .Include(t => t.TipoTurnos)
                    .Include(t => t.TipoConsultorios)
                    .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                    .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                    .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                    .Where(t => t.LiquidacionId == null) // Turnos aún no asignados a ninguna liquidación
                    .Where(t => t.FechaInicio >= desde && t.FechaInicio <= hasta)
                    .AsQueryable();

                if (profesionalId.HasValue && profesionalId.Value > 0)
                {
                    query = query.Where(t => t.TurnoProfesionales.Any(tp => tp.ProfesionalId == profesionalId.Value));
                }

                var turnos = await query.OrderBy(t => t.FechaInicio).ToListAsync();

                int sesionContador = 1;

                var resultado = turnos.Select(t =>
                {
                    var profRel = t.TurnoProfesionales.FirstOrDefault();
                    var pacRel = t.TurnoPacientes.FirstOrDefault();
                    var prestRel = t.TurnoTipoPrestaciones.FirstOrDefault();

                    int modalidad = (pacRel?.TipoObraSocialId.HasValue == true && pacRel.TipoObraSocialId.Value > 0) ? 2 : 1;
                    decimal monto = prestRel?.TipoPrestaciones?.MontoSesion ?? 15000m;
                    decimal retencion = prestRel?.TipoPrestaciones?.PorcentajeRetencion ?? 20m;

                    return new TurnoLiquidacionItemDTO
                    {
                        TurnoId = t.Id,
                        NumeroSesion = sesionContador++,
                        Fecha = t.FechaInicio.Date,
                        HoraInicio = TimeOnly.FromDateTime(t.FechaInicio),
                        HoraFin = TimeOnly.FromDateTime(t.FechaFin),
                        ConsultorioNombre = t.TipoConsultorios?.Tipo ?? "Consultorio",
                        TipoTurnoNombre = prestRel?.TipoPrestaciones?.Tipo ?? t.TipoTurnos?.Tipo ?? "Sesión",
                        EstadoTurno = t.EstadoTurno,

                        // Profesional
                        ProfesionalId = profRel?.ProfesionalId,
                        ProfesionalNombre = profRel?.Profesionales?.Nombre ?? "Profesional",

                        // Paciente
                        PacienteId = pacRel?.PacienteId,
                        PacienteNombre = pacRel?.Pacientes?.Nombre ?? "Sin Asignar",

                        // Prestación
                        TipoPrestacionId = prestRel?.TipoPrestacionId,
                        TipoPrestacionNombre = prestRel?.TipoPrestaciones?.Tipo ?? "General",

                        // Obra Social
                        TipoObraSocialId = pacRel?.TipoObraSocialId,
                        TipoObraSocialNombre = pacRel?.NombreObraSocial ?? (modalidad == 2 ? "Obra Social" : "Particular"),

                        // Modalidad y Montos
                        TipoModalidadId = modalidad,
                        MontoSugerido = monto,
                        PorcentajeRetencion = retencion
                    };
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al consultar turnos pendientes en el repositorio: {ex.Message}", ex);
            }
        }

        // Guardar o Actualizar liquidación completa con vinculación directa a los turnos
        public async Task<int> GuardarLiquidacionCompleta(GuardarLiquidacionDTO dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Liquidacion liquidacion;

                if (dto.Id == 0)
                {
                    var numeroLiq = string.IsNullOrWhiteSpace(dto.NumeroLiquidacion)
                        ? $"LIQ-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}"
                        : dto.NumeroLiquidacion.Trim();

                    liquidacion = new Liquidacion
                    {
                        NumeroLiquidacion = numeroLiq,
                        FechaDesde = DateTime.SpecifyKind(dto.FechaDesde, DateTimeKind.Utc),
                        FechaHasta = DateTime.SpecifyKind(dto.FechaHasta, DateTimeKind.Utc),
                        ProfesionalId = dto.ProfesionalId,
                        SocioId = dto.SocioId,
                        EstadoPago = dto.EstadoPago,
                        Observaciones = dto.Observaciones,
                        Observacion = dto.Observaciones ?? string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    };
                    context.Liquidaciones.Add(liquidacion);
                    await context.SaveChangesAsync();
                }
                else
                {
                    liquidacion = await context.Liquidaciones
                        .Include(l => l.DetalleLiquidaciones)
                        .FirstOrDefaultAsync(l => l.Id == dto.Id)
                        ?? throw new ApplicationException("La liquidación a actualizar no existe.");

                    liquidacion.FechaDesde = DateTime.SpecifyKind(dto.FechaDesde, DateTimeKind.Utc);
                    liquidacion.FechaHasta = DateTime.SpecifyKind(dto.FechaHasta, DateTimeKind.Utc);
                    liquidacion.ProfesionalId = dto.ProfesionalId;
                    liquidacion.SocioId = dto.SocioId;
                    liquidacion.EstadoPago = dto.EstadoPago;
                    liquidacion.Observaciones = dto.Observaciones;
                    liquidacion.Observacion = dto.Observaciones ?? string.Empty;

                    // Desvincular turnos previos para reasignar limpiamente
                    var turnosPrevios = await context.Turnos.Where(t => t.LiquidacionId == liquidacion.Id).ToListAsync();
                    foreach (var t in turnosPrevios)
                    {
                        t.LiquidacionId = null;
                        t.DetalleLiquidacionId = null;
                    }

                    context.DetalleLiquidaciones.RemoveRange(liquidacion.DetalleLiquidaciones);
                    await context.SaveChangesAsync();
                }

                decimal totalFacturacion = 0;
                decimal totalRetencion = 0;
                decimal totalSaldo = 0;

                foreach (var dDto in dto.Detalles)
                {
                    var detalle = new DetalleLiquidacion
                    {
                        LiquidacionId = liquidacion.Id,
                        TipoModalidadId = dDto.TipoModalidadId,
                        TipoPrestacionId = dDto.TipoPrestacionId,
                        TipoObraSocialId = dDto.TipoObraSocialId,
                        PacienteId = dDto.PacienteId,
                        MontoUnitario = dDto.MontoUnitario,
                        Cantidad = dDto.Cantidad,
                        MontoModuloCompleto = dDto.MontoModuloCompleto,
                        CantidadModuloCompleto = dDto.CantidadModuloCompleto,
                        MontoMedioModulo = dDto.MontoMedioModulo,
                        CantidadMedioModulo = dDto.CantidadMedioModulo,
                        MontoFacturacion = dDto.MontoFacturacion,
                        Porcentaje = dDto.Porcentaje,
                        Retencion = dDto.Retencion,
                        Saldo = dDto.Saldo,
                        EstadoPago = dDto.EstadoPago,
                        Observaciones = dDto.Observaciones,
                        Observacion = dDto.Observaciones ?? string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    };

                    context.DetalleLiquidaciones.Add(detalle);
                    await context.SaveChangesAsync();

                    totalFacturacion += detalle.MontoFacturacion;
                    totalRetencion += detalle.Retencion;
                    totalSaldo += detalle.Saldo;

                    // Sincronizar turnos: asignar LiquidacionId y DetalleLiquidacionId
                    if (dDto.TurnosIds != null && dDto.TurnosIds.Count > 0)
                    {
                        var turnosAAsociar = await context.Turnos
                            .Where(t => dDto.TurnosIds.Contains(t.Id))
                            .ToListAsync();

                        foreach (var t in turnosAAsociar)
                        {
                            t.LiquidacionId = liquidacion.Id;
                            t.DetalleLiquidacionId = detalle.Id;
                        }
                    }
                }

                liquidacion.TotalFacturacion = totalFacturacion;
                liquidacion.TotalRetencion = totalRetencion;
                liquidacion.TotalSaldo = totalSaldo;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return liquidacion.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> AnularLiquidacion(int id)
        {
            var liq = await context.Liquidaciones.FindAsync(id);
            if (liq == null) return false;

            // Marca como inactivo (anulada)
            liq.EstadoRegistro = EnumEstadoRegistro.inactivo;

            // Libera los turnos para que vuelvan a figurar como pendientes de liquidar en la agenda
            var turnos = await context.Turnos.Where(t => t.LiquidacionId == id).ToListAsync();
            foreach (var t in turnos)
            {
                t.LiquidacionId = null;
                t.DetalleLiquidacionId = null;
            }

            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ActualizarEstadoPagoLiquidacion(int liquidacionId, EnumEstadoPagoLiquidacion nuevoEstado)
        {
            var liq = await context.Liquidaciones.FindAsync(liquidacionId);
            if (liq == null) return false;

            liq.EstadoPago = nuevoEstado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarEstadoPagoDetalle(int detalleId, EnumEstadoPagoLiquidacion nuevoEstado)
        {
            var detalle = await context.DetalleLiquidaciones.FindAsync(detalleId);
            if (detalle == null) return false;

            detalle.EstadoPago = nuevoEstado;
            await context.SaveChangesAsync();
            return true;
        }
    }
}
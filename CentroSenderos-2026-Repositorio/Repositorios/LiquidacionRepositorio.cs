using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var formD = texto.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(ch));
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static bool EsTipoTurnoSinLiquidacionPorPaciente(string? tipoTurnoNombre)
        {
            var norm = NormalizarTexto(tipoTurnoNombre);
            return norm.Contains("taller") || norm.Contains("hora") || norm.Contains("modulo");
        }

        private static bool EsTipoTurnoGrupal(string? tipoTurnoNombre)
        {
            var norm = NormalizarTexto(tipoTurnoNombre);
            return norm.Contains("grupal");
        }

        private static int DeterminarModalidadDesdeTurno(Turno t, TurnoPaciente? pacRel)
        {
            var norm = NormalizarTexto(t.TipoTurnos?.Tipo);
            if (norm.Contains("taller")) return 5;
            if (norm.Contains("hora")) return 4;
            if (norm.Contains("modulo")) return 3;

            if (pacRel == null) return 1;

            var osNombre = NormalizarTexto(pacRel.NombreObraSocial ?? pacRel.TipoObraSocial?.Tipo);
            bool esParticular = !pacRel.TipoObraSocialId.HasValue
                || pacRel.TipoObraSocialId.Value <= 0
                || osNombre == "particular";

            return esParticular ? 1 : 2;
        }

        private static string NombreModalidad(int modalidadId) => modalidadId switch
        {
            2 => "Obra Social",
            3 => "Módulos",
            4 => "Por Hora",
            5 => "Taller",
            _ => "Particular"
        };

        private static TurnoLiquidacionItemDTO MapearTurnoAItem(
            Turno t,
            int? profesionalIdFiltro,
            TurnoPaciente? pacRel,
            int modalidadId,
            int numeroSesion,
            int? liquidacionIdOverride = null,
            string? numeroLiquidacionOverride = null)
        {
            var inicioLocal = t.FechaInicio.ToLocalTime();
            var finLocal = t.FechaFin.ToLocalTime();

            var profRel = t.TurnoProfesionales
                .FirstOrDefault(tp => !profesionalIdFiltro.HasValue || tp.ProfesionalId == profesionalIdFiltro.Value)
                ?? t.TurnoProfesionales.FirstOrDefault();

            var prestRel = t.TurnoTipoPrestaciones
                .FirstOrDefault(tp => tp.EstadoRegistro == EnumEstadoRegistro.activo
                    && (!profesionalIdFiltro.HasValue || tp.ProfesionalId == profesionalIdFiltro.Value))
                ?? t.TurnoTipoPrestaciones.FirstOrDefault(tp => tp.EstadoRegistro == EnumEstadoRegistro.activo);

            var tipoTurnoOrig = t.TipoTurnos?.Tipo ?? "Sesión";
            var tipoTurnoNorm = NormalizarTexto(tipoTurnoOrig);
            bool esModulo = modalidadId == 3 || tipoTurnoNorm.Contains("modulo");
            bool esModuloCompleto = esModulo && (tipoTurnoNorm.Contains("completo") || (!tipoTurnoNorm.Contains("medio") && (t.TipoTurnos?.DuracionMinutos ?? 0) >= 300));
            bool esMedioModulo = esModulo && !esModuloCompleto;

            var pacientesTurno = t.TurnoPacientes
                .OrderBy(tp => tp.Pacientes?.Nombre ?? "")
                .Select(tp => new TurnoPacienteItemLiquidacionDTO
                {
                    PacienteId = tp.PacienteId,
                    PacienteNombre = tp.Pacientes?.Nombre ?? $"Paciente #{tp.PacienteId}",
                    TipoObraSocialId = tp.TipoObraSocialId,
                    ObraSocialNombre = tp.NombreObraSocial ?? tp.TipoObraSocial?.Tipo,
                    Asistencia = tp.Asistencia
                })
                .ToList();

            decimal montoSugerido = prestRel?.TipoPrestaciones?.MontoSesion ?? 0m;
            decimal porcentajeRetencion = prestRel?.TipoPrestaciones?.PorcentajeRetencion
                ?? (modalidadId >= 3 ? 100m : 20m);

            int? obraSocialId = modalidadId == 2 ? pacRel?.TipoObraSocialId : null;
            string? obraSocialNombre = modalidadId == 2
                ? (pacRel?.NombreObraSocial ?? pacRel?.TipoObraSocial?.Tipo ?? "Obra Social")
                : (modalidadId == 1 ? "Particular" : null);

            string? pacienteNombre = modalidadId >= 3
                ? (prestRel?.TipoPrestaciones?.Tipo ?? tipoTurnoOrig)
                : (pacRel?.Pacientes?.Nombre ?? "Sin Asignar");

            return new TurnoLiquidacionItemDTO
            {
                TurnoId = t.Id,
                NumeroSesion = numeroSesion,
                Fecha = inicioLocal.Date,
                HoraInicio = TimeOnly.FromDateTime(inicioLocal),
                HoraFin = TimeOnly.FromDateTime(finLocal),
                ConsultorioNombre = t.TipoConsultorios?.Tipo ?? "Sin Consultorio",
                TipoTurnoNombre = prestRel?.TipoPrestaciones?.Tipo ?? tipoTurnoOrig,
                TipoTurnoOriginal = tipoTurnoOrig,
                EstadoTurno = t.EstadoTurno,
                EstadoPaciente = pacRel?.Asistencia,
                PacientesTurno = pacientesTurno,
                ProfesionalId = profRel?.ProfesionalId,
                ProfesionalNombre = profRel?.Profesionales?.Nombre ?? "",
                PacienteId = modalidadId >= 3 ? null : pacRel?.PacienteId,
                PacienteNombre = pacienteNombre,
                TipoPrestacionId = prestRel?.TipoPrestacionId,
                TipoPrestacionNombre = prestRel?.TipoPrestaciones?.Tipo ?? tipoTurnoOrig,
                TipoObraSocialId = obraSocialId,
                TipoObraSocialNombre = obraSocialNombre,
                TipoModalidadId = modalidadId,
                EsModuloCompleto = esModuloCompleto,
                EsMedioModulo = esMedioModulo,
                MontoSugerido = montoSugerido,
                PorcentajeRetencion = porcentajeRetencion,
                LiquidacionId = liquidacionIdOverride ?? t.LiquidacionId,
                NumeroLiquidacion = numeroLiquidacionOverride ?? t.Liquidacion?.NumeroLiquidacion
            };
        }

        // Obtener resumen para la pantalla principal con filtros de fechas y profesional
        public async Task<List<LiquidacionResumenDTO>> ObtenerResumenLiquidaciones(DateTime? fechaDesde, DateTime? fechaHasta, int? profesionalId)
        {
            var query = context.Liquidaciones
                .AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.Profesionales)
                .Include(l => l.Socios).ThenInclude(s => s!.Profesionales)
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

            var lista = await query
                .OrderByDescending(l => l.FechaDesde)
                .ToListAsync();

            return lista.Select(l =>
            {
                var detallesActivos = l.DetalleLiquidaciones
                    .Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo)
                    .ToList();

                var factPorModalidad = detallesActivos
                    .GroupBy(d => d.TipoModalidades?.Tipo ?? NombreModalidad(d.TipoModalidadId))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.MontoFacturacion));

                return new LiquidacionResumenDTO
                {
                    Id = l.Id,
                    NumeroLiquidacion = l.NumeroLiquidacion,
                    ProfesionalId = l.ProfesionalId,
                    ProfesionalNombreCompleto = l.Profesionales != null ? l.Profesionales.Nombre : "Sin Asignar",
                    UsuarioResponsable = !string.IsNullOrWhiteSpace(l.UsuarioResponsable)
                        ? l.UsuarioResponsable
                        : l.Socios?.Profesionales?.Nombre,
                    FechaDesde = l.FechaDesde,
                    FechaHasta = l.FechaHasta,
                    TotalFacturacion = l.TotalFacturacion,
                    TotalRetencion = l.TotalRetencion,
                    TotalSaldo = l.TotalSaldo,
                    TotalAbonado = l.TotalRetencion - l.TotalSaldo,
                    CobroProfesional = l.TotalFacturacion - l.TotalRetencion,
                    EstadoPago = l.EstadoPago,
                    CantidadDetalles = detallesActivos.Count,
                    Modalidades = detallesActivos
                        .Select(d => d.TipoModalidades?.Tipo ?? NombreModalidad(d.TipoModalidadId))
                        .Distinct()
                        .ToList(),
                    FacturacionPorModalidad = factPorModalidad,
                    Anulada = l.EstadoRegistro != EnumEstadoRegistro.activo
                };
            }).ToList();
        }

        // Obtener detalle completo de cabecera, filas, snapshot y sesiones asociadas
        public async Task<LiquidacionDTO?> ObtenerPorIdCompleto(int id)
        {
            var l = await context.Liquidaciones
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Profesionales)
                .Include(x => x.Socios).ThenInclude(s => s!.Profesionales)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.TipoModalidades)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.TipoPrestaciones)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.TipoObraSociales)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Pacientes)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TipoConsultorios)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TipoTurnos)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoTipoPrestaciones).ThenInclude(tp => tp.TipoPrestaciones)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TipoConsultorios)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TipoTurnos)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(x => x.DetalleLiquidaciones).ThenInclude(d => d.Turnos).ThenInclude(t => t.TurnoTipoPrestaciones).ThenInclude(tp => tp.TipoPrestaciones)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (l == null) return null;

            var sesionesCobradasPorDetalle = await context.PagoLiquidacionImputaciones
                .AsNoTracking()
                .Where(i => i.EstadoRegistro == EnumEstadoRegistro.activo
                    && i.PagoLiquidacion != null
                    && i.PagoLiquidacion.LiquidacionId == id
                    && i.PagoLiquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                    && !i.PagoLiquidacion.Anulado
                    && !i.PagoLiquidacion.Modificado)
                .GroupBy(i => i.DetalleLiquidacionId)
                .Select(g => new { DetalleId = g.Key, Sesiones = g.Sum(x => x.CantidadSesiones) })
                .ToDictionaryAsync(x => x.DetalleId, x => x.Sesiones);

            var detallesActivos = l.DetalleLiquidaciones
                .Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToList();

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
                UsuarioResponsable = !string.IsNullOrWhiteSpace(l.UsuarioResponsable)
                    ? l.UsuarioResponsable
                    : (l.Socios?.Profesionales?.Nombre ?? ""),
                EstadoPago = l.EstadoPago,
                TotalFacturacion = l.TotalFacturacion,
                TotalRetencion = l.TotalRetencion,
                TotalSaldo = l.TotalSaldo,
                Observaciones = l.Observaciones,
                Anulada = l.EstadoRegistro != EnumEstadoRegistro.activo,
                Detalles = detallesActivos.Select(d =>
                {
                    var turnosAsociados = new List<TurnoLiquidacionItemDTO>();
                    var links = d.DetalleLiquidacionTurnos
                        .Where(dt => dt.EstadoRegistro == EnumEstadoRegistro.activo && dt.Turno != null)
                        .OrderBy(dt => dt.Turno!.FechaInicio)
                        .ToList();

                    if (links.Count > 0)
                    {
                        int nro = 1;
                        foreach (var link in links)
                        {
                            var t = link.Turno!;
                            int? pacBuscado = link.PacienteId ?? d.PacienteId;
                            var pacRel = pacBuscado.HasValue
                                ? t.TurnoPacientes.FirstOrDefault(tp => tp.PacienteId == pacBuscado.Value)
                                : t.TurnoPacientes.FirstOrDefault();

                            turnosAsociados.Add(MapearTurnoAItem(
                                t,
                                l.ProfesionalId,
                                pacRel,
                                d.TipoModalidadId,
                                nro++,
                                l.Id,
                                l.NumeroLiquidacion));
                        }
                    }
                    else if (d.Turnos.Count > 0)
                    {
                        int nro = 1;
                        foreach (var t in d.Turnos.Where(t => t.EstadoRegistro == EnumEstadoRegistro.activo).OrderBy(t => t.FechaInicio))
                        {
                            var pacRel = d.PacienteId.HasValue
                                ? t.TurnoPacientes.FirstOrDefault(tp => tp.PacienteId == d.PacienteId.Value)
                                : t.TurnoPacientes.FirstOrDefault();

                            turnosAsociados.Add(MapearTurnoAItem(
                                t,
                                l.ProfesionalId,
                                pacRel,
                                d.TipoModalidadId,
                                nro++,
                                l.Id,
                                l.NumeroLiquidacion));
                        }
                    }

                    return new DetalleLiquidacionDTO
                    {
                        Id = d.Id,
                        LiquidacionId = d.LiquidacionId,
                        TipoModalidadId = d.TipoModalidadId,
                        TipoModalidadNombre = d.TipoModalidades?.Tipo ?? NombreModalidad(d.TipoModalidadId),
                        TipoPrestacionId = d.TipoPrestacionId,
                        TipoPrestacionNombre = d.TipoPrestaciones?.Tipo ?? "",
                        TipoObraSocialId = d.TipoObraSocialId,
                        TipoObraSocialNombre = d.TipoObraSociales?.Tipo ?? "",
                        PacienteId = d.PacienteId,
                        PacienteNombreCompleto = d.Pacientes?.Nombre ?? (d.TipoModalidadId >= 3 ? "Uso Institucional / Alquiler" : "Sin Paciente"),
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
                        TipoModalidadIdOriginal = d.TipoModalidadIdOriginal,
                        TipoPrestacionIdOriginal = d.TipoPrestacionIdOriginal,
                        TipoPrestacionNombreOriginal = d.TipoPrestacionNombreOriginal,
                        TipoObraSocialIdOriginal = d.TipoObraSocialIdOriginal,
                        TipoObraSocialNombreOriginal = d.TipoObraSocialNombreOriginal,
                        PacienteIdOriginal = d.PacienteIdOriginal,
                        PacienteNombreOriginal = d.PacienteNombreOriginal,
                        MontoUnitarioOriginal = d.MontoUnitarioOriginal,
                        CantidadOriginal = d.CantidadOriginal,
                        MontoModuloCompletoOriginal = d.MontoModuloCompletoOriginal,
                        CantidadModuloCompletoOriginal = d.CantidadModuloCompletoOriginal,
                        MontoMedioModuloOriginal = d.MontoMedioModuloOriginal,
                        CantidadMedioModuloOriginal = d.CantidadMedioModuloOriginal,
                        PorcentajeOriginal = d.PorcentajeOriginal,
                        ValoresModificadosManualmente = d.ValoresModificadosManualmente,
                        SinCoincidenciaAgenda = d.SinCoincidenciaAgenda,
                        ResumenCambiosManuales = d.ResumenCambiosManuales,
                        SesionesCobradas = sesionesCobradasPorDetalle.TryGetValue(d.Id, out var sc) ? sc : 0m,
                        CantidadTurnosAsignados = turnosAsociados.Count,
                        TurnosAsociados = turnosAsociados
                    };
                }).ToList()
            };
        }

        public async Task<LiquidacionDTO?> ObtenerPorNumero(string numeroLiquidacion)
        {
            var l = await context.Liquidaciones.FirstOrDefaultAsync(x => x.NumeroLiquidacion == numeroLiquidacion);
            if (l == null) return null;
            return await ObtenerPorIdCompleto(l.Id);
        }

        // Obtener turnos de una fila de detalle (soporta DetalleLiquidacionTurnos y legado Turno.DetalleLiquidacionId)
        public async Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPorDetalleId(int detalleLiquidacionId)
        {
            var detalle = await context.DetalleLiquidaciones
                .AsNoTracking()
                .AsSplitQuery()
                .Include(d => d.Liquidaciones)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TipoConsultorios)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TipoTurnos)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(d => d.DetalleLiquidacionTurnos).ThenInclude(dt => dt.Turno!).ThenInclude(t => t.TurnoTipoPrestaciones).ThenInclude(tp => tp.TipoPrestaciones)
                .FirstOrDefaultAsync(d => d.Id == detalleLiquidacionId);

            if (detalle == null) return new List<TurnoLiquidacionItemDTO>();

            var links = detalle.DetalleLiquidacionTurnos
                .Where(dt => dt.EstadoRegistro == EnumEstadoRegistro.activo && dt.Turno != null)
                .OrderBy(dt => dt.Turno!.FechaInicio)
                .ToList();

            var resultado = new List<TurnoLiquidacionItemDTO>();
            int sesionNro = 1;

            if (links.Count > 0)
            {
                foreach (var link in links)
                {
                    var t = link.Turno!;
                    int? pacBuscado = link.PacienteId ?? detalle.PacienteId;
                    var pacRel = pacBuscado.HasValue
                        ? t.TurnoPacientes.FirstOrDefault(tp => tp.PacienteId == pacBuscado.Value)
                        : t.TurnoPacientes.FirstOrDefault();

                    resultado.Add(MapearTurnoAItem(
                        t,
                        detalle.Liquidaciones?.ProfesionalId,
                        pacRel,
                        detalle.TipoModalidadId,
                        sesionNro++,
                        detalle.LiquidacionId,
                        detalle.Liquidaciones?.NumeroLiquidacion));
                }
                return resultado;
            }

            var turnosLegado = await context.Turnos
                .AsNoTracking()
                .Include(t => t.Liquidacion)
                .Include(t => t.TipoConsultorios)
                .Include(t => t.TipoTurnos)
                .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                .Where(t => t.DetalleLiquidacionId == detalleLiquidacionId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            foreach (var t in turnosLegado)
            {
                var pacRel = detalle.PacienteId.HasValue
                    ? t.TurnoPacientes.FirstOrDefault(tp => tp.PacienteId == detalle.PacienteId.Value)
                    : t.TurnoPacientes.FirstOrDefault();

                resultado.Add(MapearTurnoAItem(
                    t,
                    detalle.Liquidaciones?.ProfesionalId,
                    pacRel,
                    detalle.TipoModalidadId,
                    sesionNro++,
                    detalle.LiquidacionId,
                    detalle.Liquidaciones?.NumeroLiquidacion));
            }

            return resultado;
        }

        public async Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPorIds(List<int> turnoIds, int? pacienteId = null, int? profesionalId = null)
        {
            if (turnoIds == null || turnoIds.Count == 0)
                return new List<TurnoLiquidacionItemDTO>();

            var turnos = await context.Turnos
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.Liquidacion)
                .Include(t => t.TipoConsultorios)
                .Include(t => t.TipoTurnos)
                .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                .Where(t => turnoIds.Contains(t.Id))
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            var resultado = new List<TurnoLiquidacionItemDTO>();
            int sesionNro = 1;

            foreach (var t in turnos)
            {
                var pacRel = pacienteId.HasValue && pacienteId.Value > 0
                    ? t.TurnoPacientes.FirstOrDefault(tp => tp.PacienteId == pacienteId.Value)
                    : t.TurnoPacientes.FirstOrDefault();

                int modalidadId = DeterminarModalidadDesdeTurno(t, pacRel);
                resultado.Add(MapearTurnoAItem(t, profesionalId, pacRel, modalidadId, sesionNro++));
            }

            return resultado;
        }

        // Obtener turnos pendientes sin liquidar (soporta sesiones grupales por paciente/modalidad, individuales, taller, por hora, módulos)
        public async Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPendientes(int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                var desdeBase = (fechaDesde ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
                var hastaBase = (fechaHasta ?? desdeBase.AddMonths(1).AddDays(-1)).Date.AddDays(1).AddTicks(-1);

                var desde = DateTime.SpecifyKind(desdeBase, DateTimeKind.Utc);
                var hasta = DateTime.SpecifyKind(hastaBase, DateTimeKind.Utc);

                var query = context.Turnos
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(t => t.Liquidacion)
                    .Include(t => t.TipoTurnos)
                    .Include(t => t.TipoConsultorios)
                    .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                    .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                    .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                    .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                    .Where(t => t.EstadoRegistro == EnumEstadoRegistro.activo)
                    .Where(t => t.EstadoTurno != EnumEstadoTurno.disponible)
                    .Where(t => t.FechaInicio >= desde && t.FechaInicio <= hasta)
                    .AsQueryable();

                if (profesionalId.HasValue && profesionalId.Value > 0)
                {
                    query = query.Where(t => t.TurnoProfesionales.Any(tp => tp.ProfesionalId == profesionalId.Value));
                }

                var turnos = await query.OrderBy(t => t.FechaInicio).ToListAsync();
                var turnoIds = turnos.Select(t => t.Id).ToList();

                // Vínculos activos ya liquidados en DetalleLiquidacionTurnos
                var linksActivos = await context.DetalleLiquidacionTurnos
                    .AsNoTracking()
                    .Where(dt => turnoIds.Contains(dt.TurnoId)
                        && dt.EstadoRegistro == EnumEstadoRegistro.activo
                        && dt.DetalleLiquidacion != null
                        && dt.DetalleLiquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                        && dt.DetalleLiquidacion.Liquidaciones != null
                        && dt.DetalleLiquidacion.Liquidaciones.EstadoRegistro == EnumEstadoRegistro.activo)
                    .Select(dt => new
                    {
                        dt.TurnoId,
                        dt.PacienteId,
                        ProfesionalId = dt.DetalleLiquidacion!.Liquidaciones!.ProfesionalId
                    })
                    .ToListAsync();

                var resultado = new List<TurnoLiquidacionItemDTO>();
                int sesionContador = 1;

                foreach (var t in turnos)
                {
                    int? profIdEfectivo = profesionalId.HasValue && profesionalId.Value > 0
                        ? profesionalId.Value
                        : t.TurnoProfesionales.FirstOrDefault()?.ProfesionalId;

                    bool tieneLinksParaTurno = linksActivos.Any(x => x.TurnoId == t.Id && (!profIdEfectivo.HasValue || x.ProfesionalId == profIdEfectivo.Value));
                    bool legadoLiquidado = !tieneLinksParaTurno
                        && t.LiquidacionId.HasValue
                        && t.Liquidacion != null
                        && t.Liquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                        && (!profIdEfectivo.HasValue || t.Liquidacion.ProfesionalId == profIdEfectivo.Value);

                    if (legadoLiquidado)
                        continue;

                    string? tipoTurnoStr = t.TipoTurnos?.Tipo;

                    if (EsTipoTurnoSinLiquidacionPorPaciente(tipoTurnoStr))
                    {
                        // Taller / Por Hora / Módulos: no se liquida por paciente
                        bool yaLiquidado = linksActivos.Any(x =>
                            x.TurnoId == t.Id &&
                            (!profIdEfectivo.HasValue || x.ProfesionalId == profIdEfectivo.Value));

                        if (yaLiquidado)
                            continue;

                        int modId = DeterminarModalidadDesdeTurno(t, null);
                        resultado.Add(MapearTurnoAItem(t, profIdEfectivo, null, modId, sesionContador++));
                    }
                    else
                    {
                        // Sesión Individual o Sesión Grupal: se liquida por paciente y por su modalidad (Particular / Obra Social)
                        if (t.TurnoPacientes.Count == 0)
                        {
                            bool yaLiquidado = linksActivos.Any(x =>
                                x.TurnoId == t.Id &&
                                (!profIdEfectivo.HasValue || x.ProfesionalId == profIdEfectivo.Value));

                            if (!yaLiquidado)
                            {
                                resultado.Add(MapearTurnoAItem(t, profIdEfectivo, null, 1, sesionContador++));
                            }
                            continue;
                        }

                        foreach (var pacRel in t.TurnoPacientes.OrderBy(p => p.Pacientes?.Nombre ?? ""))
                        {
                            bool yaLiquidadoPaciente = linksActivos.Any(x =>
                                x.TurnoId == t.Id &&
                                (!profIdEfectivo.HasValue || x.ProfesionalId == profIdEfectivo.Value) &&
                                (x.PacienteId == pacRel.PacienteId || x.PacienteId == null));

                            if (yaLiquidadoPaciente)
                                continue;

                            int modId = DeterminarModalidadDesdeTurno(t, pacRel);
                            resultado.Add(MapearTurnoAItem(t, profIdEfectivo, pacRel, modId, sesionContador++));
                        }
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al consultar turnos pendientes en el repositorio: {ex.Message}", ex);
            }
        }

        // Validar si un detalle cargado manualmente coincide con turnos pendientes, ya liquidados o con ninguno
        public async Task<ValidarDetalleManualResponseDTO> ValidarDetalleManualContraAgenda(ValidarDetalleManualRequestDTO req)
        {
            if (req.ProfesionalId <= 0)
            {
                return new ValidarDetalleManualResponseDTO
                {
                    EstadoCoincidencia = "SinCoincidencia",
                    BloquearAgregado = false,
                    Mensaje = "Seleccione un profesional para validar contra la agenda."
                };
            }

            var desde = DateTime.SpecifyKind(req.FechaDesde.Date, DateTimeKind.Utc);
            var hasta = DateTime.SpecifyKind(req.FechaHasta.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var turnos = await context.Turnos
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.Liquidacion)
                .Include(t => t.TipoTurnos)
                .Include(t => t.TipoConsultorios)
                .Include(t => t.TurnoProfesionales).ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.Pacientes)
                .Include(t => t.TurnoPacientes).ThenInclude(tp => tp.TipoObraSocial)
                .Include(t => t.TurnoTipoPrestaciones).ThenInclude(tpr => tpr.TipoPrestaciones)
                .Where(t => t.EstadoRegistro == EnumEstadoRegistro.activo)
                .Where(t => t.EstadoTurno != EnumEstadoTurno.disponible)
                .Where(t => t.FechaInicio >= desde && t.FechaInicio <= hasta)
                .Where(t => t.TurnoProfesionales.Any(tp => tp.ProfesionalId == req.ProfesionalId))
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();

            var turnoIds = turnos.Select(t => t.Id).ToList();

            var linksActivos = await context.DetalleLiquidacionTurnos
                .AsNoTracking()
                .Include(dt => dt.DetalleLiquidacion!).ThenInclude(d => d.Liquidaciones)
                .Where(dt => turnoIds.Contains(dt.TurnoId)
                    && dt.EstadoRegistro == EnumEstadoRegistro.activo
                    && dt.DetalleLiquidacion != null
                    && dt.DetalleLiquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                    && dt.DetalleLiquidacion.Liquidaciones != null
                    && dt.DetalleLiquidacion.Liquidaciones.EstadoRegistro == EnumEstadoRegistro.activo
                    && dt.DetalleLiquidacion.Liquidaciones.ProfesionalId == req.ProfesionalId)
                .ToListAsync();

            var coincidentesPendientes = new List<TurnoLiquidacionItemDTO>();
            var coincidentesLiquidados = new List<(int TurnoId, string NumeroLiquidacion)>();
            var turnosMismoPacienteDiferenteCriterio = new List<TurnoLiquidacionItemDTO>();

            int nro = 1;
            foreach (var t in turnos)
            {
                bool sinPacientePorModalidad = EsTipoTurnoSinLiquidacionPorPaciente(t.TipoTurnos?.Tipo);

                if (sinPacientePorModalidad)
                {
                    var item = MapearTurnoAItem(t, req.ProfesionalId, null, DeterminarModalidadDesdeTurno(t, null), nro++);

                    bool coincide = item.TipoModalidadId == req.TipoModalidadId
                        && (req.TipoModalidadId == 3
                            || !req.TipoPrestacionId.HasValue
                            || req.TipoPrestacionId.Value <= 0
                            || item.TipoPrestacionId == req.TipoPrestacionId.Value);

                    if (!coincide) continue;

                    var linkOtraLiq = linksActivos.FirstOrDefault(x =>
                        x.TurnoId == t.Id && x.DetalleLiquidacion!.LiquidacionId != req.LiquidacionId);

                    if (linkOtraLiq != null)
                    {
                        coincidentesLiquidados.Add((t.Id, linkOtraLiq.DetalleLiquidacion!.Liquidaciones!.NumeroLiquidacion));
                        continue;
                    }

                    bool legadoOtraLiq = !linksActivos.Any(x => x.TurnoId == t.Id)
                        && t.LiquidacionId.HasValue
                        && t.Liquidacion != null
                        && t.Liquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                        && t.LiquidacionId.Value != req.LiquidacionId
                        && t.Liquidacion.ProfesionalId == req.ProfesionalId;

                    if (legadoOtraLiq)
                    {
                        coincidentesLiquidados.Add((t.Id, t.Liquidacion!.NumeroLiquidacion));
                        continue;
                    }

                    if (req.TurnosYaEnLiquidacionActual.Contains(t.Id))
                    {
                        coincidentesLiquidados.Add((t.Id, "esta liquidación"));
                        continue;
                    }

                    coincidentesPendientes.Add(item);
                }
                else
                {
                    foreach (var pacRel in t.TurnoPacientes)
                    {
                        int modId = DeterminarModalidadDesdeTurno(t, pacRel);
                        var item = MapearTurnoAItem(t, req.ProfesionalId, pacRel, modId, nro++);

                        bool mismoPaciente = req.PacienteId.HasValue && req.PacienteId.Value > 0 && item.PacienteId == req.PacienteId.Value;
                        bool mismaModalidad = item.TipoModalidadId == req.TipoModalidadId;
                        bool mismaPrestacion = !req.TipoPrestacionId.HasValue || req.TipoPrestacionId.Value <= 0 || item.TipoPrestacionId == req.TipoPrestacionId.Value;
                        bool mismaObraSocial = req.TipoModalidadId != 2
                            || item.TipoObraSocialId == req.TipoObraSocialId;

                        if (mismoPaciente && (!mismaModalidad || !mismaPrestacion || !mismaObraSocial))
                        {
                            turnosMismoPacienteDiferenteCriterio.Add(item);
                        }

                        if (!(mismoPaciente && mismaModalidad && mismaPrestacion && mismaObraSocial))
                            continue;

                        var linkOtraLiq = linksActivos.FirstOrDefault(x =>
                            x.TurnoId == t.Id
                            && (x.PacienteId == pacRel.PacienteId || x.PacienteId == null)
                            && x.DetalleLiquidacion!.LiquidacionId != req.LiquidacionId);

                        if (linkOtraLiq != null)
                        {
                            coincidentesLiquidados.Add((t.Id, linkOtraLiq.DetalleLiquidacion!.Liquidaciones!.NumeroLiquidacion));
                            continue;
                        }

                        bool legadoOtraLiq = !linksActivos.Any(x => x.TurnoId == t.Id)
                            && t.LiquidacionId.HasValue
                            && t.Liquidacion != null
                            && t.Liquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                            && t.LiquidacionId.Value != req.LiquidacionId
                            && t.Liquidacion.ProfesionalId == req.ProfesionalId;

                        if (legadoOtraLiq)
                        {
                            coincidentesLiquidados.Add((t.Id, t.Liquidacion!.NumeroLiquidacion));
                            continue;
                        }

                        if (req.TurnosYaEnLiquidacionActual.Contains(t.Id))
                        {
                            coincidentesLiquidados.Add((t.Id, "esta liquidación"));
                            continue;
                        }

                        coincidentesPendientes.Add(item);
                    }
                }
            }

            if (coincidentesPendientes.Count > 0)
            {
                // Renumerar sesiones 1..N
                for (int i = 0; i < coincidentesPendientes.Count; i++)
                    coincidentesPendientes[i].NumeroSesion = i + 1;

                var primero = coincidentesPendientes[0];
                return new ValidarDetalleManualResponseDTO
                {
                    EstadoCoincidencia = "CoincidePendiente",
                    BloquearAgregado = false,
                    Mensaje = $"Sincronizado automáticamente con {coincidentesPendientes.Count} turno(s) coincidente(s) del calendario.",
                    TurnosIdsCoincidentes = coincidentesPendientes.Select(x => x.TurnoId).ToList(),
                    TurnosCoincidentes = coincidentesPendientes,
                    CantidadTurnosAgenda = coincidentesPendientes.Count,
                    CantidadModulosCompletosAgenda = coincidentesPendientes.Count(x => x.EsModuloCompleto),
                    CantidadMediosModulosAgenda = coincidentesPendientes.Count(x => x.EsMedioModulo),
                    MontoSugeridoAgenda = primero.MontoSugerido,
                    PorcentajeRetencionAgenda = primero.PorcentajeRetencion
                };
            }

            if (coincidentesLiquidados.Count > 0)
            {
                var nums = string.Join(", ", coincidentesLiquidados.Select(x => x.NumeroLiquidacion).Distinct());
                return new ValidarDetalleManualResponseDTO
                {
                    EstadoCoincidencia = "YaLiquidado",
                    BloquearAgregado = true,
                    Mensaje = $"No se puede agregar este detalle porque los turnos coincidentes ({coincidentesLiquidados.Count}) ya fueron liquidados en: {nums}."
                };
            }

            string msgSinCoincidencia = turnosMismoPacienteDiferenteCriterio.Count > 0
                ? $"No coincide exactamente con ningún turno del calendario para estos datos (el paciente tiene {turnosMismoPacienteDiferenteCriterio.Count} turno(s) en el período con otra prestación, modalidad u obra social)."
                : "Este detalle no coincide con ningún turno registrado en el calendario para el profesional y período seleccionados.";

            return new ValidarDetalleManualResponseDTO
            {
                EstadoCoincidencia = "SinCoincidencia",
                BloquearAgregado = false,
                Mensaje = msgSinCoincidencia
            };
        }

        // Guardar o Actualizar liquidación completa preservando IDs de detalles con pagos y sincronizando DetalleLiquidacionTurnos
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
                        SocioId = dto.SocioId.HasValue && dto.SocioId.Value > 0 ? dto.SocioId.Value : null,
                        UsuarioResponsable = string.IsNullOrWhiteSpace(dto.UsuarioResponsable) ? "Sistema" : dto.UsuarioResponsable.Trim(),
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
                            .ThenInclude(d => d.DetalleLiquidacionTurnos)
                        .FirstOrDefaultAsync(l => l.Id == dto.Id)
                        ?? throw new ApplicationException("La liquidación a actualizar no existe.");

                    // Al modificar una liquidación NO se cambia período, profesional ni socio/usuario responsable
                    liquidacion.Observaciones = dto.Observaciones;
                    liquidacion.Observacion = dto.Observaciones ?? string.Empty;

                    var idsEnDto = dto.Detalles.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();
                    var detallesAEliminar = liquidacion.DetalleLiquidaciones
                        .Where(d => !idsEnDto.Contains(d.Id))
                        .ToList();

                    if (detallesAEliminar.Count > 0)
                    {
                        var idsEliminar = detallesAEliminar.Select(d => d.Id).ToList();

                        bool algunoConPagosVigentes = await context.PagoLiquidacionImputaciones
                            .AnyAsync(i => idsEliminar.Contains(i.DetalleLiquidacionId)
                                && i.EstadoRegistro == EnumEstadoRegistro.activo
                                && i.PagoLiquidacion != null
                                && i.PagoLiquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                                && !i.PagoLiquidacion.Anulado
                                && !i.PagoLiquidacion.Modificado);

                        if (algunoConPagosVigentes)
                            throw new ApplicationException("No se puede eliminar un detalle que posee cobros vigentes registrados. Anule el cobro primero.");

                        var imputacionesHistoricas = await context.PagoLiquidacionImputaciones
                            .Where(i => idsEliminar.Contains(i.DetalleLiquidacionId))
                            .ToListAsync();

                        if (imputacionesHistoricas.Count > 0)
                            throw new ApplicationException("No se puede eliminar físicamente un detalle que figura en el historial de cobros (aunque hayan sido anulados o modificados).");

                        var linksEliminar = await context.DetalleLiquidacionTurnos
                            .Where(dt => idsEliminar.Contains(dt.DetalleLiquidacionId))
                            .ToListAsync();
                        context.DetalleLiquidacionTurnos.RemoveRange(linksEliminar);

                        var turnosLegadoLimpiar = await context.Turnos
                            .Where(t => t.DetalleLiquidacionId.HasValue && idsEliminar.Contains(t.DetalleLiquidacionId.Value))
                            .ToListAsync();
                        foreach (var t in turnosLegadoLimpiar)
                        {
                            t.LiquidacionId = null;
                            t.DetalleLiquidacionId = null;
                        }

                        context.DetalleLiquidaciones.RemoveRange(detallesAEliminar);
                        await context.SaveChangesAsync();
                    }
                }

                // Desvincular punteros legado de turnos previamente apuntando a esta liquidación (se re-sincronizan abajo)
                var turnosPreviosLegado = await context.Turnos
                    .Where(t => t.LiquidacionId == liquidacion.Id)
                    .ToListAsync();
                foreach (var t in turnosPreviosLegado)
                {
                    t.LiquidacionId = null;
                    t.DetalleLiquidacionId = null;
                }

                // Cargar pagos vigentes reales por detalle para preservar Saldos exactamente al editar
                var abonadoPorDetalle = await context.PagoLiquidacionImputaciones
                    .Where(i => i.EstadoRegistro == EnumEstadoRegistro.activo
                        && i.PagoLiquidacion != null
                        && i.PagoLiquidacion.LiquidacionId == liquidacion.Id
                        && i.PagoLiquidacion.EstadoRegistro == EnumEstadoRegistro.activo
                        && !i.PagoLiquidacion.Anulado
                        && !i.PagoLiquidacion.Modificado)
                    .GroupBy(i => i.DetalleLiquidacionId)
                    .Select(g => new { DetalleId = g.Key, Abonado = g.Sum(x => x.MontoImputado) })
                    .ToDictionaryAsync(x => x.DetalleId, x => x.Abonado);

                decimal totalFacturacion = 0;
                decimal totalRetencion = 0;
                decimal totalSaldo = 0;

                foreach (var dDto in dto.Detalles)
                {
                    DetalleLiquidacion detalle;
                    if (dDto.Id > 0)
                    {
                        detalle = liquidacion.DetalleLiquidaciones.FirstOrDefault(x => x.Id == dDto.Id)
                            ?? throw new ApplicationException($"Detalle de liquidación #{dDto.Id} no encontrado.");

                        detalle.TipoModalidadId = dDto.TipoModalidadId;
                        detalle.TipoPrestacionId = dDto.TipoPrestacionId;
                        detalle.TipoObraSocialId = dDto.TipoObraSocialId;
                        detalle.PacienteId = dDto.PacienteId;
                        detalle.MontoUnitario = dDto.MontoUnitario;
                        detalle.Cantidad = dDto.Cantidad;
                        detalle.MontoModuloCompleto = dDto.MontoModuloCompleto;
                        detalle.CantidadModuloCompleto = dDto.CantidadModuloCompleto;
                        detalle.MontoMedioModulo = dDto.MontoMedioModulo;
                        detalle.CantidadMedioModulo = dDto.CantidadMedioModulo;
                        detalle.MontoFacturacion = dDto.MontoFacturacion;
                        detalle.Porcentaje = dDto.Porcentaje;
                        detalle.Retencion = dDto.Retencion;

                        decimal abonadoActual = abonadoPorDetalle.TryGetValue(detalle.Id, out var ab) ? ab : 0m;
                        detalle.Saldo = Math.Max(0m, detalle.Retencion - abonadoActual);
                        detalle.EstadoPago = detalle.Saldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                            : (abonadoActual > 0m ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);

                        detalle.Observaciones = dDto.Observaciones;
                        detalle.Observacion = dDto.Observaciones ?? string.Empty;

                        detalle.TipoModalidadIdOriginal = dDto.TipoModalidadIdOriginal;
                        detalle.TipoPrestacionIdOriginal = dDto.TipoPrestacionIdOriginal;
                        detalle.TipoPrestacionNombreOriginal = dDto.TipoPrestacionNombreOriginal;
                        detalle.TipoObraSocialIdOriginal = dDto.TipoObraSocialIdOriginal;
                        detalle.TipoObraSocialNombreOriginal = dDto.TipoObraSocialNombreOriginal;
                        detalle.PacienteIdOriginal = dDto.PacienteIdOriginal;
                        detalle.PacienteNombreOriginal = dDto.PacienteNombreOriginal;
                        detalle.MontoUnitarioOriginal = dDto.MontoUnitarioOriginal;
                        detalle.CantidadOriginal = dDto.CantidadOriginal;
                        detalle.MontoModuloCompletoOriginal = dDto.MontoModuloCompletoOriginal;
                        detalle.CantidadModuloCompletoOriginal = dDto.CantidadModuloCompletoOriginal;
                        detalle.MontoMedioModuloOriginal = dDto.MontoMedioModuloOriginal;
                        detalle.CantidadMedioModuloOriginal = dDto.CantidadMedioModuloOriginal;
                        detalle.PorcentajeOriginal = dDto.PorcentajeOriginal;
                        detalle.ValoresModificadosManualmente = dDto.ValoresModificadosManualmente;
                        detalle.SinCoincidenciaAgenda = dDto.SinCoincidenciaAgenda;
                        detalle.ResumenCambiosManuales = dDto.ResumenCambiosManuales;

                        // Reemplazar links de turnos para este detalle
                        var linksPrevios = await context.DetalleLiquidacionTurnos
                            .Where(dt => dt.DetalleLiquidacionId == detalle.Id)
                            .ToListAsync();
                        context.DetalleLiquidacionTurnos.RemoveRange(linksPrevios);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        detalle = new DetalleLiquidacion
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
                            Saldo = dDto.Retencion,
                            EstadoPago = dDto.Retencion <= 0 ? EnumEstadoPagoLiquidacion.Pagado : EnumEstadoPagoLiquidacion.Pendiente,
                            Observaciones = dDto.Observaciones,
                            Observacion = dDto.Observaciones ?? string.Empty,
                            TipoModalidadIdOriginal = dDto.TipoModalidadIdOriginal,
                            TipoPrestacionIdOriginal = dDto.TipoPrestacionIdOriginal,
                            TipoPrestacionNombreOriginal = dDto.TipoPrestacionNombreOriginal,
                            TipoObraSocialIdOriginal = dDto.TipoObraSocialIdOriginal,
                            TipoObraSocialNombreOriginal = dDto.TipoObraSocialNombreOriginal,
                            PacienteIdOriginal = dDto.PacienteIdOriginal,
                            PacienteNombreOriginal = dDto.PacienteNombreOriginal,
                            MontoUnitarioOriginal = dDto.MontoUnitarioOriginal,
                            CantidadOriginal = dDto.CantidadOriginal,
                            MontoModuloCompletoOriginal = dDto.MontoModuloCompletoOriginal,
                            CantidadModuloCompletoOriginal = dDto.CantidadModuloCompletoOriginal,
                            MontoMedioModuloOriginal = dDto.MontoMedioModuloOriginal,
                            CantidadMedioModuloOriginal = dDto.CantidadMedioModuloOriginal,
                            PorcentajeOriginal = dDto.PorcentajeOriginal,
                            ValoresModificadosManualmente = dDto.ValoresModificadosManualmente,
                            SinCoincidenciaAgenda = dDto.SinCoincidenciaAgenda,
                            ResumenCambiosManuales = dDto.ResumenCambiosManuales,
                            EstadoRegistro = EnumEstadoRegistro.activo
                        };

                        context.DetalleLiquidaciones.Add(detalle);
                        await context.SaveChangesAsync();
                    }

                    totalFacturacion += detalle.MontoFacturacion;
                    totalRetencion += detalle.Retencion;
                    totalSaldo += detalle.Saldo;

                    // Sincronizar DetalleLiquidacionTurnos y punteros legado cuando corresponda
                    var idsTurnos = (dDto.TurnosIds != null && dDto.TurnosIds.Count > 0)
                        ? dDto.TurnosIds.Distinct().ToList()
                        : dDto.TurnosAsociados.Select(t => t.TurnoId).Distinct().ToList();

                    if (idsTurnos.Count > 0)
                    {
                        var turnosAAsociar = await context.Turnos
                            .Include(t => t.TipoTurnos)
                            .Include(t => t.TurnoPacientes)
                            .Where(t => idsTurnos.Contains(t.Id))
                            .ToListAsync();

                        foreach (var t in turnosAAsociar)
                        {
                            context.DetalleLiquidacionTurnos.Add(new DetalleLiquidacionTurno
                            {
                                DetalleLiquidacionId = detalle.Id,
                                TurnoId = t.Id,
                                PacienteId = detalle.TipoModalidadId >= 3 ? null : detalle.PacienteId,
                                EstadoRegistro = EnumEstadoRegistro.activo,
                                Observacion = string.Empty
                            });

                            bool esGrupal = EsTipoTurnoGrupal(t.TipoTurnos?.Tipo);
                            if (!esGrupal || t.TurnoPacientes.Count <= 1)
                            {
                                t.LiquidacionId = liquidacion.Id;
                                t.DetalleLiquidacionId = detalle.Id;
                            }
                        }
                    }
                }

                liquidacion.TotalFacturacion = totalFacturacion;
                liquidacion.TotalRetencion = totalRetencion;
                liquidacion.TotalSaldo = totalSaldo;
                liquidacion.EstadoPago = totalSaldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                    : (totalSaldo < totalRetencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);

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
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var liq = await context.Liquidaciones
                    .Include(l => l.DetalleLiquidaciones)
                    .FirstOrDefaultAsync(l => l.Id == id);
                if (liq == null) return false;

                liq.EstadoRegistro = EnumEstadoRegistro.inactivo;

                var detalleIds = liq.DetalleLiquidaciones.Select(d => d.Id).ToList();
                foreach (var d in liq.DetalleLiquidaciones)
                    d.EstadoRegistro = EnumEstadoRegistro.inactivo;

                var links = await context.DetalleLiquidacionTurnos
                    .Where(dt => detalleIds.Contains(dt.DetalleLiquidacionId))
                    .ToListAsync();
                foreach (var link in links)
                    link.EstadoRegistro = EnumEstadoRegistro.inactivo;

                var turnos = await context.Turnos.Where(t => t.LiquidacionId == id).ToListAsync();
                foreach (var t in turnos)
                {
                    t.LiquidacionId = null;
                    t.DetalleLiquidacionId = null;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> EliminarLiquidacion(int id)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var liq = await context.Liquidaciones
                    .Include(l => l.DetalleLiquidaciones)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (liq == null) return false;

                var detalleIds = liq.DetalleLiquidaciones.Select(d => d.Id).ToList();

                var turnos = await context.Turnos.Where(t => t.LiquidacionId == id).ToListAsync();
                foreach (var t in turnos)
                {
                    t.LiquidacionId = null;
                    t.DetalleLiquidacionId = null;
                }

                var links = await context.DetalleLiquidacionTurnos
                    .Where(dt => detalleIds.Contains(dt.DetalleLiquidacionId))
                    .ToListAsync();
                if (links.Count > 0)
                    context.DetalleLiquidacionTurnos.RemoveRange(links);

                var pagos = await context.PagosLiquidaciones
                    .Include(p => p.MetodosPago)
                    .Include(p => p.Imputaciones)
                    .Where(p => p.LiquidacionId == id)
                    .ToListAsync();

                foreach (var p in pagos)
                {
                    if (p.Imputaciones.Count > 0)
                        context.PagoLiquidacionImputaciones.RemoveRange(p.Imputaciones);
                    if (p.MetodosPago.Count > 0)
                        context.PagoLiquidacionMetodos.RemoveRange(p.MetodosPago);
                }

                if (pagos.Count > 0)
                    context.PagosLiquidaciones.RemoveRange(pagos);

                context.DetalleLiquidaciones.RemoveRange(liq.DetalleLiquidaciones);
                context.Liquidaciones.Remove(liq);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        // ==========================================
        // HISTORIAL DE PAGOS Y COBROS DE LIQUIDACIÓN
        // ==========================================
        public async Task<List<PagoLiquidacionHistorialDTO>> ObtenerHistorialPagos(int liquidacionId)
        {
            var pagos = await context.PagosLiquidaciones
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.MetodosPago)
                .Include(p => p.Imputaciones).ThenInclude(i => i.DetalleLiquidacion!).ThenInclude(d => d.Pacientes)
                .Include(p => p.Imputaciones).ThenInclude(i => i.DetalleLiquidacion!).ThenInclude(d => d.TipoPrestaciones)
                .Include(p => p.Imputaciones).ThenInclude(i => i.DetalleLiquidacion!).ThenInclude(d => d.TipoModalidades)
                .Where(p => p.LiquidacionId == liquidacionId && p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            return pagos.Select(p => new PagoLiquidacionHistorialDTO
            {
                Id = p.Id,
                LiquidacionId = p.LiquidacionId,
                FechaPago = p.FechaPago.ToLocalTime(),
                MontoTotal = p.MontoTotal,
                Observaciones = p.Observacion,
                RegistradoPor = p.RegistradoPor,
                Anulado = p.Anulado,
                FechaAnulacion = p.FechaAnulacion?.ToLocalTime(),
                AnuladoPor = p.AnuladoPor,
                MotivoAnulacion = p.MotivoAnulacion,
                Modificado = p.Modificado,
                FechaModificacion = p.FechaModificacion?.ToLocalTime(),
                ModificadoPor = p.ModificadoPor,
                MotivoModificacion = p.MotivoModificacion,
                PagoOriginalId = p.PagoOriginalId,
                Metodos = p.MetodosPago.Select(m => new CobroMetodoPagoDTO
                {
                    Id = m.Id,
                    MetodoPago = m.MetodoPago,
                    MetodoPagoNombre = m.MetodoPagoNombre,
                    Monto = m.Monto,
                    CuentaOrigen = m.CuentaOrigen,
                    NumeroComprobante = m.NumeroComprobante
                }).ToList(),
                Imputaciones = p.Imputaciones.Select(i => new CobroDetalleImputacionDTO
                {
                    DetalleLiquidacionId = i.DetalleLiquidacionId,
                    PacienteNombre = i.DetalleLiquidacion?.Pacientes?.Nombre ?? "Uso Institucional / Alquiler",
                    PrestacionNombre = i.DetalleLiquidacion?.TipoPrestaciones?.Tipo ?? "",
                    ModalidadNombre = i.DetalleLiquidacion?.TipoModalidades?.Tipo ?? "",
                    MontoImputado = i.MontoImputado,
                    CantidadSesionesCobradas = i.CantidadSesiones,
                    Nota = i.Nota
                }).ToList()
            }).ToList();
        }

        public async Task<bool> RegistrarCobro(RegistrarCobroLiquidacionDTO dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var liq = await context.Liquidaciones
                    .Include(l => l.DetalleLiquidaciones)
                    .FirstOrDefaultAsync(l => l.Id == dto.LiquidacionId);

                if (liq == null) return false;

                decimal montoTotal = dto.Metodos.Sum(m => m.Monto);

                var pago = new PagoLiquidacion
                {
                    LiquidacionId = dto.LiquidacionId,
                    FechaPago = DateTime.UtcNow,
                    MontoTotal = montoTotal,
                    Observacion = dto.Observaciones ?? string.Empty,
                    RegistradoPor = !string.IsNullOrWhiteSpace(dto.RegistradoPor) ? dto.RegistradoPor : "Usuario",
                    Anulado = false,
                    Modificado = false,
                    EstadoRegistro = EnumEstadoRegistro.activo,
                    MetodosPago = dto.Metodos.Select(m => new PagoLiquidacionMetodo
                    {
                        MetodoPago = m.MetodoPago,
                        MetodoPagoNombre = !string.IsNullOrWhiteSpace(m.MetodoPagoNombre) ? m.MetodoPagoNombre : m.MetodoPago.ToString(),
                        Monto = m.Monto,
                        CuentaOrigen = m.CuentaOrigen,
                        NumeroComprobante = m.NumeroComprobante,
                        Observacion = string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }).ToList(),
                    Imputaciones = dto.Imputaciones.Select(imp => new PagoLiquidacionDetalleImputacion
                    {
                        DetalleLiquidacionId = imp.DetalleLiquidacionId,
                        MontoImputado = imp.MontoImputado,
                        CantidadSesiones = imp.CantidadSesionesCobradas,
                        Nota = imp.Nota,
                        Observacion = string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }).ToList()
                };

                context.PagosLiquidaciones.Add(pago);
                await context.SaveChangesAsync();

                foreach (var imp in dto.Imputaciones)
                {
                    var det = liq.DetalleLiquidaciones.FirstOrDefault(d => d.Id == imp.DetalleLiquidacionId);
                    if (det != null)
                    {
                        det.Saldo = Math.Max(0m, det.Saldo - imp.MontoImputado);
                        det.EstadoPago = det.Saldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                            : (det.Saldo < det.Retencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);
                    }
                }

                liq.TotalSaldo = liq.DetalleLiquidaciones.Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo).Sum(d => d.Saldo);
                liq.EstadoPago = liq.TotalSaldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                    : (liq.TotalSaldo < liq.TotalRetencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AnularCobro(AnularPagoLiquidacionDTO dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var pago = await context.PagosLiquidaciones
                    .Include(p => p.Imputaciones)
                    .FirstOrDefaultAsync(p => p.Id == dto.PagoId);

                if (pago == null || pago.Anulado || pago.Modificado) return false;

                pago.Anulado = true;
                pago.FechaAnulacion = DateTime.UtcNow;
                pago.AnuladoPor = !string.IsNullOrWhiteSpace(dto.AnuladoPor) ? dto.AnuladoPor : "Usuario";
                pago.MotivoAnulacion = dto.Motivo;

                var liq = await context.Liquidaciones
                    .Include(l => l.DetalleLiquidaciones)
                    .FirstOrDefaultAsync(l => l.Id == pago.LiquidacionId);

                if (liq != null)
                {
                    foreach (var imp in pago.Imputaciones)
                    {
                        var det = liq.DetalleLiquidaciones.FirstOrDefault(d => d.Id == imp.DetalleLiquidacionId);
                        if (det != null)
                        {
                            det.Saldo = Math.Min(det.Retencion, det.Saldo + imp.MontoImputado);
                            det.EstadoPago = det.Saldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                                : (det.Saldo < det.Retencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);
                        }
                    }

                    liq.TotalSaldo = liq.DetalleLiquidaciones.Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo).Sum(d => d.Saldo);
                    liq.EstadoPago = liq.TotalSaldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                        : (liq.TotalSaldo < liq.TotalRetencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ModificarCobro(ModificarPagoLiquidacionDTO dto)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var pagoAnterior = await context.PagosLiquidaciones
                    .Include(p => p.Imputaciones)
                    .FirstOrDefaultAsync(p => p.Id == dto.PagoId);

                if (pagoAnterior == null || pagoAnterior.Anulado || pagoAnterior.Modificado)
                    return false;

                var liq = await context.Liquidaciones
                    .Include(l => l.DetalleLiquidaciones)
                    .FirstOrDefaultAsync(l => l.Id == pagoAnterior.LiquidacionId);

                if (liq == null) return false;

                // 1. Revertir saldo del cobro anterior antes de aplicar la versión modificada
                foreach (var imp in pagoAnterior.Imputaciones)
                {
                    var det = liq.DetalleLiquidaciones.FirstOrDefault(d => d.Id == imp.DetalleLiquidacionId);
                    if (det != null)
                    {
                        det.Saldo = Math.Min(det.Retencion, det.Saldo + imp.MontoImputado);
                    }
                }

                // 2. Marcar el cobro anterior como Modificado conservándolo visible en el historial
                pagoAnterior.Modificado = true;
                pagoAnterior.FechaModificacion = DateTime.UtcNow;
                pagoAnterior.ModificadoPor = !string.IsNullOrWhiteSpace(dto.ModificadoPor) ? dto.ModificadoPor : "Usuario";
                pagoAnterior.MotivoModificacion = dto.Motivo;

                // 3. Crear el nuevo registro de cobro enlazado al original
                decimal nuevoMontoTotal = dto.Metodos.Sum(m => m.Monto);
                var pagoNuevo = new PagoLiquidacion
                {
                    LiquidacionId = liq.Id,
                    FechaPago = DateTime.UtcNow,
                    MontoTotal = nuevoMontoTotal,
                    Observacion = string.IsNullOrWhiteSpace(dto.Observaciones)
                        ? $"Modificación de cobro #{pagoAnterior.Id}: {dto.Motivo}"
                        : dto.Observaciones,
                    RegistradoPor = pagoAnterior.ModificadoPor,
                    Anulado = false,
                    Modificado = false,
                    PagoOriginalId = pagoAnterior.Id,
                    EstadoRegistro = EnumEstadoRegistro.activo,
                    MetodosPago = dto.Metodos.Select(m => new PagoLiquidacionMetodo
                    {
                        MetodoPago = m.MetodoPago,
                        MetodoPagoNombre = !string.IsNullOrWhiteSpace(m.MetodoPagoNombre) ? m.MetodoPagoNombre : m.MetodoPago.ToString(),
                        Monto = m.Monto,
                        CuentaOrigen = m.CuentaOrigen,
                        NumeroComprobante = m.NumeroComprobante,
                        Observacion = string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }).ToList(),
                    Imputaciones = dto.Imputaciones.Select(imp => new PagoLiquidacionDetalleImputacion
                    {
                        DetalleLiquidacionId = imp.DetalleLiquidacionId,
                        MontoImputado = imp.MontoImputado,
                        CantidadSesiones = imp.CantidadSesionesCobradas,
                        Nota = imp.Nota,
                        Observacion = string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    }).ToList()
                };

                context.PagosLiquidaciones.Add(pagoNuevo);
                await context.SaveChangesAsync();

                // 4. Imputar los montos actualizados sobre cada detalle
                foreach (var imp in dto.Imputaciones)
                {
                    var det = liq.DetalleLiquidaciones.FirstOrDefault(d => d.Id == imp.DetalleLiquidacionId);
                    if (det == null)
                        throw new ApplicationException($"El detalle #{imp.DetalleLiquidacionId} no pertenece a la liquidación.");

                    if (imp.MontoImputado > det.Saldo + 0.01m)
                        throw new ApplicationException($"El monto imputado (${imp.MontoImputado:N2}) supera el saldo disponible (${det.Saldo:N2}).");

                    det.Saldo = Math.Max(0m, det.Saldo - imp.MontoImputado);
                }

                foreach (var det in liq.DetalleLiquidaciones.Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo))
                {
                    det.EstadoPago = det.Saldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                        : (det.Saldo < det.Retencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);
                }

                liq.TotalSaldo = liq.DetalleLiquidaciones.Where(d => d.EstadoRegistro == EnumEstadoRegistro.activo).Sum(d => d.Saldo);
                liq.EstadoPago = liq.TotalSaldo <= 0m ? EnumEstadoPagoLiquidacion.Pagado
                    : (liq.TotalSaldo < liq.TotalRetencion ? EnumEstadoPagoLiquidacion.Parcial : EnumEstadoPagoLiquidacion.Pendiente);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
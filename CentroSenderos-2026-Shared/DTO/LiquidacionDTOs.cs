using System;
using System.Collections.Generic;
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    // DTO para la grilla de la pantalla principal (Agrupada por Profesional y Rango de Fechas)
    public class LiquidacionResumenDTO
    {
        public int Id { get; set; }
        public string NumeroLiquidacion { get; set; } = string.Empty;
        public int ProfesionalId { get; set; }
        public string ProfesionalNombreCompleto { get; set; } = string.Empty;
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public decimal TotalFacturacion { get; set; }
        public decimal TotalRetencion { get; set; }
        public decimal TotalSaldo { get; set; }
        public decimal TotalAbonado { get; set; }
        public decimal CobroProfesional { get; set; }
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; }
        public int CantidadDetalles { get; set; }
        public List<string> Modalidades { get; set; } = new();
        public bool Anulada { get; set; } = false;
    }

    // DTO completo de Cabecera + Detalles
    public class LiquidacionDTO
    {
        public int Id { get; set; }
        public string NumeroLiquidacion { get; set; } = string.Empty;
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int ProfesionalId { get; set; }
        public string ProfesionalNombreCompleto { get; set; } = string.Empty;
        public int SocioId { get; set; }
        public string SocioNombre { get; set; } = string.Empty;
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; }
        public decimal TotalFacturacion { get; set; }
        public decimal TotalRetencion { get; set; }
        public decimal TotalSaldo { get; set; }
        public string? Observaciones { get; set; }
        public bool Anulada { get; set; } = false;
        public List<DetalleLiquidacionDTO> Detalles { get; set; } = new();
    }

    // Alias para compatibilidad total con llamadas que busquen LiquidacionDetalleDTO
    public class LiquidacionDetalleDTO : LiquidacionDTO { }

    // DTO para cada fila de detalle (Modalidad, Paciente/Taller, Montos y Retenciones)
    public class DetalleLiquidacionDTO
    {
        public int Id { get; set; }
        public int LiquidacionId { get; set; }
        public int TipoModalidadId { get; set; }
        public string TipoModalidadNombre { get; set; } = string.Empty;

        public int? TipoPrestacionId { get; set; }
        public string? TipoPrestacionNombre { get; set; }

        public int? TipoObraSocialId { get; set; }
        public string? TipoObraSocialNombre { get; set; }

        public int? PacienteId { get; set; }
        public string? PacienteNombreCompleto { get; set; }
        public string? PacienteNombre
        {
            get => PacienteNombreCompleto;
            set => PacienteNombreCompleto = value;
        }

        public decimal MontoUnitario { get; set; }
        public int Cantidad { get; set; }

        public decimal? MontoModuloCompleto { get; set; }
        public int? CantidadModuloCompleto { get; set; }
        public decimal? MontoMedioModulo { get; set; }
        public int? CantidadMedioModulo { get; set; }

        public decimal MontoFacturacion { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Retencion { get; set; }
        public decimal Saldo { get; set; }
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; }
        public string? Observaciones { get; set; }

        // Cantidad de turnos asignados a este detalle
        public int CantidadTurnosAsignados { get; set; }
    }

    // DTO para el sub-detalle de turnos con compatibilidad de nombres
    public class TurnoLiquidacionItemDTO
    {
        public int TurnoId { get; set; }
        public int NumeroSesion { get; set; }
        public DateTime Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string ConsultorioNombre { get; set; } = string.Empty;
        public string TipoTurnoNombre { get; set; } = string.Empty;
        public EnumEstadoTurno EstadoTurno { get; set; }

        // Profesional (AGREGADO PARA SOLUCIONAR CS1061)
        public int? ProfesionalId { get; set; }
        public string? ProfesionalNombre { get; set; }
        public string? ProfesionalNombreCompleto
        {
            get => ProfesionalNombre;
            set => ProfesionalNombre = value;
        }

        // Paciente
        public int? PacienteId { get; set; }
        public string? PacienteNombre { get; set; }
        public string? PacienteNombreCompleto
        {
            get => PacienteNombre;
            set => PacienteNombre = value;
        }

        // Prestación
        public int? TipoPrestacionId { get; set; }
        public string? TipoPrestacionNombre { get; set; }

        // Obra Social (con alias para ObraSocialId)
        public int? TipoObraSocialId { get; set; }
        public int? ObraSocialId
        {
            get => TipoObraSocialId;
            set => TipoObraSocialId = value;
        }
        public string? TipoObraSocialNombre { get; set; }
        public string? ObraSocialNombre
        {
            get => TipoObraSocialNombre;
            set => TipoObraSocialNombre = value;
        }

        // Modalidad
        public int TipoModalidadId { get; set; } = 1;

        // Montos (con alias para MontoSesion)
        public decimal MontoSugerido { get; set; }
        public decimal MontoSesion
        {
            get => MontoSugerido;
            set => MontoSugerido = value;
        }

        public decimal PorcentajeRetencion { get; set; } = 20;
    }

    // DTO para crear o actualizar Liquidación con sus detalles
    public class GuardarLiquidacionDTO
    {
        public int Id { get; set; } // 0 para nuevo
        public string? NumeroLiquidacion { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int ProfesionalId { get; set; }
        public int SocioId { get; set; }
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; } = EnumEstadoPagoLiquidacion.Pendiente;
        public string? Observaciones { get; set; }

        public List<GuardarDetalleLiquidacionDTO> Detalles { get; set; } = new();
    }

    public class GuardarDetalleLiquidacionDTO
    {
        public int Id { get; set; }
        public int TipoModalidadId { get; set; }
        public string TipoModalidadNombre { get; set; } = string.Empty;

        public int? TipoPrestacionId { get; set; }
        public string? TipoPrestacionNombre { get; set; }

        public int? TipoObraSocialId { get; set; }
        public string? TipoObraSocialNombre { get; set; }

        public int? PacienteId { get; set; }
        public string? PacienteNombreCompleto { get; set; }
        public string? PacienteNombre
        {
            get => PacienteNombreCompleto;
            set => PacienteNombreCompleto = value;
        }

        public decimal MontoUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal? MontoModuloCompleto { get; set; }
        public int? CantidadModuloCompleto { get; set; }
        public decimal? MontoMedioModulo { get; set; }
        public int? CantidadMedioModulo { get; set; }
        public decimal MontoFacturacion { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Retencion { get; set; }
        public decimal Saldo { get; set; }
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; }
        public string? Observaciones { get; set; }

        // IDs de los turnos que se asocian a este detalle
        public List<int> TurnosIds { get; set; } = new();
    }
}
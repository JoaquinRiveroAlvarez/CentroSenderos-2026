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

    // Alias para compatibilidad con código que use LiquidacionListadoDTO
    public class LiquidacionListadoDTO : LiquidacionResumenDTO { }

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
        public DateTime? FechaAnulacion { get; set; }
        public string? MotivoAnulacion { get; set; }
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

        // Propiedades auxiliares con alias para compatibilidad total
        public decimal RetencionCentroPesos
        {
            get => Retencion;
            set => Retencion = value;
        }
        public decimal MontoTotal
        {
            get => MontoFacturacion;
            set => MontoFacturacion = value;
        }
        public decimal PorcentajeRetencionCentro
        {
            get => Porcentaje;
            set => Porcentaje = value;
        }
        public decimal MontoCobroProfesional => MontoFacturacion - Retencion;
        public decimal MontoAbonado => Retencion - Saldo;
        public decimal SaldoPendiente
        {
            get => Saldo;
            set => Saldo = value;
        }
        public int CantidadSesiones
        {
            get => Cantidad;
            set => Cantidad = value;
        }
        public string ModalidadAtencion
        {
            get => TipoModalidadNombre;
            set => TipoModalidadNombre = value;
        }
        public string ObraSocialNombre
        {
            get => TipoObraSocialNombre ?? "Particular";
            set => TipoObraSocialNombre = value;
        }
        public string TipoTurnoNombre
        {
            get => TipoPrestacionNombre ?? "";
            set => TipoPrestacionNombre = value;
        }

        // Subdetalle de turnos
        public List<TurnoLiquidacionItemDTO> TurnosAsociados { get; set; } = new();
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
        public string Prestacion
        {
            get => TipoTurnoNombre;
            set => TipoTurnoNombre = value;
        }
        public EnumEstadoTurno EstadoTurno { get; set; }

        // Profesional
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
        public string ObraSocial
        {
            get => TipoObraSocialNombre ?? "Particular";
            set => TipoObraSocialNombre = value;
        }

        // Modalidad
        public int TipoModalidadId { get; set; } = 1;

        // Montos (con alias para MontoSesion y Monto)
        public decimal MontoSugerido { get; set; }
        public decimal MontoSesion
        {
            get => MontoSugerido;
            set => MontoSugerido = value;
        }
        public decimal Monto
        {
            get => MontoSugerido;
            set => MontoSugerido = value;
        }

        public decimal PorcentajeRetencion { get; set; } = 20;

        // Liquidación asignada para navegación
        public int? LiquidacionId { get; set; }
        public string? NumeroLiquidacion { get; set; }
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

    // === DTOs PARA HISTORIAL Y REGISTRO DE PAGOS / COBROS ===
    public class CobroMetodoPagoDTO
    {
        public int Id { get; set; }
        public MetodoDePago MetodoPago { get; set; } = MetodoDePago.Efectivo;
        public string MetodoPagoNombre { get; set; } = "Efectivo";
        public decimal Monto { get; set; }
        public string? CuentaOrigen { get; set; }
        public string? NumeroComprobante { get; set; }
    }

    public class CobroDetalleImputacionDTO
    {
        public int DetalleLiquidacionId { get; set; }
        public decimal MontoImputado { get; set; }
        public decimal MontoACobrar
        {
            get => MontoImputado;
            set => MontoImputado = value;
        }
        public int CantidadSesionesCobradas { get; set; } = 1;
        public int CantidadSesiones
        {
            get => CantidadSesionesCobradas;
            set => CantidadSesionesCobradas = value;
        }
        public string? PacienteNombre { get; set; }
        public string? PrestacionNombre { get; set; }
        public string? Nota { get; set; }
    }

    public class RegistrarCobroLiquidacionDTO
    {
        public int LiquidacionId { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        public string? RegistradoPor { get; set; }
        public string? Observaciones { get; set; }
        public List<CobroMetodoPagoDTO> Metodos { get; set; } = new();
        public List<CobroMetodoPagoDTO> MetodosPago
        {
            get => Metodos;
            set => Metodos = value;
        }
        public List<CobroDetalleImputacionDTO> Imputaciones { get; set; } = new();
    }

    public class PagoLiquidacionHistorialDTO
    {
        public int Id { get; set; }
        public int LiquidacionId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string? Observaciones { get; set; }
        public string? RegistradoPor { get; set; }
        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public string? AnuladoPor { get; set; }
        public string? MotivoAnulacion { get; set; }
        public string? Motivo
        {
            get => MotivoAnulacion;
            set => MotivoAnulacion = value;
        }
        public List<CobroMetodoPagoDTO> Metodos { get; set; } = new();
        public List<CobroMetodoPagoDTO> MetodosPago
        {
            get => Metodos;
            set => Metodos = value;
        }
        public List<CobroDetalleImputacionDTO> Imputaciones { get; set; } = new();
    }

    public class AnularPagoLiquidacionDTO
    {
        public int PagoId { get; set; }
        public int PagoLiquidacionId
        {
            get => PagoId;
            set => PagoId = value;
        }
        public string AnuladoPor { get; set; } = "";
        public string Motivo { get; set; } = "";
        public string MotivoAnulacion
        {
            get => Motivo;
            set => Motivo = value;
        }
    }
}
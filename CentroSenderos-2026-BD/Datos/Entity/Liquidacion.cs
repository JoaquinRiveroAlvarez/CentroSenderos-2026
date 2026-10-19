using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(NumeroLiquidacion), Name = "NumeroLiquidacion_UQ", IsUnique = true)]
    public class Liquidacion : EntityBase
    {
        [Required(ErrorMessage = "El Número de Liquidación es obligatorio")]
        [MaxLength(30, ErrorMessage = "Máxima cantidad de caracteres: 30")]
        public string NumeroLiquidacion { get; set; } = string.Empty; // Ej: "LIQ-2718811"

        [Required(ErrorMessage = "La Fecha Desde es obligatoria")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La Fecha Hasta es obligatoria")]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "El profesional es obligatorio")]
        public int ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }

        [Required(ErrorMessage = "El socio es obligatorio")]
        public int SocioId { get; set; }
        public Socio? Socios { get; set; }

        // Estado general de la liquidación del profesional para este período
        [Required(ErrorMessage = "El estado de pago es obligatorio")]
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; } = EnumEstadoPagoLiquidacion.Pendiente;

        // Totales consolidados de la liquidación
        [Precision(18, 2)]
        public decimal TotalFacturacion { get; set; } = 0;

        [Precision(18, 2)]
        public decimal TotalRetencion { get; set; } = 0;

        [Precision(18, 2)]
        public decimal TotalSaldo { get; set; } = 0;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Colecciones relacionadas
        public List<DetalleLiquidacion> DetalleLiquidaciones { get; set; } = new();
        public List<Turno> Turnos { get; set; } = new();
    }
}
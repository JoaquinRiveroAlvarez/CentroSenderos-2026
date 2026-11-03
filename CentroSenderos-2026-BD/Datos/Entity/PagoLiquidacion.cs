using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class PagoLiquidacion : EntityBase
    {
        [Required(ErrorMessage = "La liquidación es obligatoria")]
        public int LiquidacionId { get; set; }
        public Liquidacion? Liquidacion { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Precision(18, 2)]
        public decimal MontoTotal { get; set; } = 0;

        [MaxLength(100)]
        public string? RegistradoPor { get; set; }

        public bool Anulado { get; set; } = false;

        public DateTime? FechaAnulacion { get; set; }

        [MaxLength(100)]
        public string? AnuladoPor { get; set; }

        [MaxLength(500)]
        public string? MotivoAnulacion { get; set; }

        // Colecciones relacionadas
        public List<PagoLiquidacionMetodo> MetodosPago { get; set; } = new();
        public List<PagoLiquidacionDetalleImputacion> Imputaciones { get; set; } = new();
    }
}
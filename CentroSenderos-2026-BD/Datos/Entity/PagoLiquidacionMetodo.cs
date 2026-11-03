using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class PagoLiquidacionMetodo : EntityBase
    {
        [Required(ErrorMessage = "El pago de liquidación es obligatorio")]
        public int PagoLiquidacionId { get; set; }
        public PagoLiquidacion? PagoLiquidacion { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public MetodoDePago MetodoPago { get; set; } = MetodoDePago.Efectivo;

        [MaxLength(50)]
        public string MetodoPagoNombre { get; set; } = "Efectivo";

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Precision(18, 2)]
        public decimal Monto { get; set; } = 0;

        [MaxLength(100)]
        public string? CuentaOrigen { get; set; }

        [MaxLength(100)]
        public string? NumeroComprobante { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class PagoLiquidacionDetalleImputacion : EntityBase
    {
        [Required(ErrorMessage = "El pago de liquidación es obligatorio")]
        public int PagoLiquidacionId { get; set; }
        public PagoLiquidacion? PagoLiquidacion { get; set; }

        [Required(ErrorMessage = "El detalle de liquidación es obligatorio")]
        public int DetalleLiquidacionId { get; set; }
        public DetalleLiquidacion? DetalleLiquidacion { get; set; }

        [Required(ErrorMessage = "El monto imputado es obligatorio")]
        [Precision(18, 2)]
        public decimal MontoImputado { get; set; } = 0;

        public int CantidadSesiones { get; set; } = 1;

        [MaxLength(200)]
        public string? Nota { get; set; }
    }
}
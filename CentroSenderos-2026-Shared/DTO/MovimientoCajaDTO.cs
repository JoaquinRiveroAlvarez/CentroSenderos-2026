using System;

namespace CentroSenderos_2026_Shared.DTO
{
    public class MovimientoCajaDTO
    {
        public int OrigenId { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public decimal Importe { get; set; }
    }
}

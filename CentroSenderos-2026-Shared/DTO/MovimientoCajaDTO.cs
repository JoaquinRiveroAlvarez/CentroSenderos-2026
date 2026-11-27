using System;

namespace CentroSenderos_2026_Shared.DTO
{
    public enum TipoMovimientoCaja
    {
        Cobro,
        Anulacion,
        PagoGasto,
        Reintegro,
        AnulacionGasto,
        ReversionReintegro
    }

    public class MovimientoCajaDTO
    {
        public TipoMovimientoCaja Tipo { get; set; }
        public int OrigenId { get; set; }
        public int? LiquidacionId { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public decimal Importe { get; set; }
    }

    public class GastoMovimientoCajaDTO
    {
        public int GastoId { get; set; }
        public bool Revertido { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}

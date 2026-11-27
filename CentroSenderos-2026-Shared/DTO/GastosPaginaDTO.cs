using System;
using System.Collections.Generic;

namespace CentroSenderos_2026_Shared.DTO
{
    public class FiltroGastosDTO
    {
        public bool Anulados { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int TipoId { get; set; }
        public int PagadorId { get; set; }
        public string? Texto { get; set; }
        public bool Descendente { get; set; } = true;
        public EstadoReintegroGasto? EstadoReintegro { get; set; }
        public EstadoComprobanteGasto? EstadoComprobante { get; set; }
        public int SocioPendienteId { get; set; }
        public bool Pendientes { get; set; }
        public bool PorRevisar { get; set; }
        public int Pagina { get; set; } = 1;
    }

    public enum EstadoComprobanteGasto
    {
        ConComprobante,
        SinComprobante
    }

    public class GastosPaginaDTO
    {
        public int CajaId { get; set; }
        public int Pagina { get; set; } = 1;
        public int Paginas { get; set; } = 1;
        public int Cantidad { get; set; }
        public int Pendientes { get; set; }
        public int PorRevisar { get; set; }
        public int Saldados { get; set; }
        public decimal Total { get; set; }
        public decimal TotalSeleccion { get; set; }
        public decimal PagadoCaja { get; set; }
        public decimal Adelantado { get; set; }
        public decimal Reintegrado { get; set; }
        public List<TotalGastosPorMesYTipoDTO> TotalesPorMesYTipo { get; set; } = new();
        public List<GastoListadoDTO> Gastos { get; set; } = new();
        public List<GastoListadoDTO> Prioritarios { get; set; } = new();
        public List<TipoListadoDTO> Tipos { get; set; } = new();
        public List<GastoSocioListadoDTO> Pagadores { get; set; } = new();
        public List<SaldoGastoSocioDTO> Socios { get; set; } = new();
    }

    public class TotalGastosPorMesYTipoDTO
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string TipoGasto { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class SaldoGastoSocioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Adelantado { get; set; }
        public decimal Recibido { get; set; }
        public decimal Pendiente => Adelantado - Recibido;
    }
}

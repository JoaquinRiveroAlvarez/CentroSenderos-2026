using System;
using System.Collections.Generic;


namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoListadoDTO
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int TipoGastoId { get; set; }

        public string TipoGasto { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public bool TieneHistorial { get; set; }

        public List<GastoSocioListadoDTO> GastoSocios { get; set; } = new();
    }
}
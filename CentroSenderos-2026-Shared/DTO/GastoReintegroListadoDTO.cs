using System;

namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoReintegroListadoDTO
    {
        public int ReintegroSocioId { get; set; }

        public DateTime Fecha { get; set; }

        public int SocioPagadorId { get; set; }

        public int SocioReceptorId { get; set; }

        public decimal Monto { get; set; }
    }
}
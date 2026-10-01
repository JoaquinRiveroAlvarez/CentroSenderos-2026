using System;

namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoHistorialDTO
    {
        public int Id { get; set; }

        public int GastoId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public DateTime FechaCambio { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public GastoListadoDTO DatosAnteriores { get; set; } = new();

        public GastoListadoDTO DatosNuevos { get; set; } = new();
    }
}
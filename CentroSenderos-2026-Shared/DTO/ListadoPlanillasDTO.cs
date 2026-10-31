using System;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ListadoPlanillasDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int TipoPlanillaId { get; set; }

        public string TipoPlanillaNombre { get; set; } = string.Empty;

        public string NombreGenerado { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }

        public DateTime FechaSubida { get; set; }
    }
}
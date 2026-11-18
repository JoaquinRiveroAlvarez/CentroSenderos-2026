namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoPrestacionListadoDTO
    {
        public int Id { get; set; }
        public required string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public required string Cod { get; set; } = string.Empty;
        public required decimal MontoSesion { get; set; } = 0;
        // Opcional durante la clasificación del catálogo existente.
        public int? TipoAreaId { get; set; }
        public string? TipoAreaNombre { get; set; }

        public decimal PorcentajeRetencion { get; set; } = 20;
    }
}

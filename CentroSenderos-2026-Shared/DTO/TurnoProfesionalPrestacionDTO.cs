namespace CentroSenderos_2026_Shared.DTO
{
    public class TurnoProfesionalPrestacionDTO
    {
        public int ProfesionalId { get; set; }
        public string NombreProfesional { get; set; } = string.Empty;

        public int? TipoAreaId { get; set; }
        public string? NombreTipoArea { get; set; }

        public int TipoPrestacionId { get; set; }
        public string NombreTipoPrestacion { get; set; } = string.Empty;
    }
}

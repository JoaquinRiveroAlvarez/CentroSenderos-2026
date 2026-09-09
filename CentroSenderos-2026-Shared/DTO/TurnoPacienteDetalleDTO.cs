namespace CentroSenderos_2026_Shared.DTO
{
    public class TurnoPacienteDetalleDTO
    {
        public int PacienteId { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;

        public int? TipoObraSocialId { get; set; }

        public string? NombreObraSocial { get; set; }
    }
}
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class SocioDTO
    {
        public int Id { get; set; }
        public int? ProfesionalId { get; set; }
        public bool EsCaja { get; set; }
        public string Profesional { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}
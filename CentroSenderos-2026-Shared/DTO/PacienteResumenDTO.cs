using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteResumenDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;

        public int NumeroAfiliado { get; set; }

        public int TipoObraSocialId { get; set; }
        public string TipoObraSocialNombre { get; set; } = string.Empty;

        public int TipoDiagnosticoId { get; set; }
        public string TipoDiagnosticoNombre { get; set; } = string.Empty; // ✅ nuevo campo
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}

using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteResumenDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }
        public bool TieneCud { get; set; }

        public int NumeroAfiliado { get; set; }

        // Compatibilidad con pacientes anteriores.
        public string Telefono { get; set; } = string.Empty;

        public List<PacienteTelefonoDTO> Telefonos { get; set; } = new();

        public int TipoObraSocialId { get; set; }
        public string TipoObraSocialNombre { get; set; } = string.Empty;

        public int TipoDiagnosticoId { get; set; }
        public string TipoDiagnosticoNombre { get; set; } = string.Empty;

        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}
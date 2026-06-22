using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public int TipoObraSocialId { get; set; }
        public int NumeroAfiliado { get; set; }
        public int TipoDiagnosticoId { get; set; }
        public string? Telefono { get; set; }
        public string? Domicilio { get; set; }
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }

}

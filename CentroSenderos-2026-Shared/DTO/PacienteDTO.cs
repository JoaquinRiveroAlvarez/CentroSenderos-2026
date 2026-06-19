using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteDTO
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; } 
        public required string DNI { get; set; }
        public int TipoObraSocialId { get; set; }
        public required int NumeroAfiliado { get; set; }
        public int TipoDiagnosticoId { get; set; }
        public required string Telefono { get; set; }
        public required string Domicilio { get; set; }
        public EnumEstadoRegistro EstadoRegistro { get; set; }

    }
}

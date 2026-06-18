using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteEditarDTO
    {
        public required string Nombre { get; set; }
        public required string DNI { get; set; }
        public required EnumSeleccionarGenero Genero { get; set; }
        public required DateTime FechaNacimiento { get; set; }
        public int TipoObraSocialId { get; set; }
        public required int NumeroAfiliado { get; set; }
        public int TipoDiagnosticoId { get; set; }
        public int ProfesionalId { get; set; }
        public required string Telefono { get; set; }
        public required string Domicilio { get; set; }
        public required string CorreoElectronico { get; set; }
        public int TipoDocumentoId { get; set; }
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}

using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteDTO
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; } 
        public required string DNI { get; set; }
        public required EnumSeleccionarGenero Genero { get; set; }
        public required DateTime FechaNacimiento { get; set; }
        public int TipoObraSocialId { get; set; }
        public required int NumeroAfiliado { get; set; }
        public required int HistoriaClinica { get; set; }
        public int TipoDiagnosticoId { get; set; }
        public int ProfesionalId { get; set; }
        public required string Telefono { get; set; }
        public required string Domicilio { get; set; }
        public required string CorreoElectronico { get; set; }
        public EnumEstadoRegistro EstadoRegistro { get; set; }
        public int TipoDocumentoId { get; set; }

        //public TipoObraSocial? TipoObraSociales { get; set; }
        //public TipoDiagnostico? TipoDiagnosticos { get; set; }
    }
}

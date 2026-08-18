using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [MaxLength(10, ErrorMessage = "El DNI no puede exceder los 10 caracteres")]
        public string DNI { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La Obra Social es obligatoria")]
        public int TipoObraSocialId { get; set; }

        public string TipoObraSocialNombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "El Número de Afiliado es obligatorio")]
        public int NumeroAfiliado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Diagnóstico es obligatorio")]
        public int TipoDiagnosticoId { get; set; }

        public string TipoDiagnosticoNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Teléfono es obligatorio")]
        [MaxLength(30, ErrorMessage = "El Teléfono no puede exceder los 30 caracteres")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El Domicilio es obligatorio")]
        [MaxLength(30, ErrorMessage = "El Domicilio no puede exceder los 30 caracteres")]
        public string? Domicilio { get; set; }

        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}
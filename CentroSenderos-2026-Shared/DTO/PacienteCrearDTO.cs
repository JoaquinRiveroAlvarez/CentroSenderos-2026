using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteCrearDTO
    {
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Nombre no puede exceder los 50 caracteres")]
        public required string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [MaxLength(10, ErrorMessage = "El DNI no puede exceder los 10 caracteres")]
        public required string DNI { get; set; } = string.Empty;
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime? FechaNacimiento { get; set; }
        public bool TieneCud { get; set; }

        //[Range(1, int.MaxValue, ErrorMessage = "El Número de Afiliado es obligatorio")]
        //public int NumeroAfiliado { get; set; }

        public string Telefono { get; set; } = string.Empty;
        public List<PacienteTelefonoDTO> Telefonos { get; set; } = new();

        [Required(ErrorMessage = "El Domicilio es obligatorio")]
        [MaxLength(30, ErrorMessage = "El Domicilio no puede exceder los 30 caracteres")]
        public required string Domicilio { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La Obra Social es obligatoria")]
        public int TipoObraSocialId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El Diagnóstico es obligatorio")]
        public int TipoDiagnosticoId { get; set; }
    }
}
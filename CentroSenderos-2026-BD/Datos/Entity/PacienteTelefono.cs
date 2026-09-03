using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class PacienteTelefono : EntityBase
    {
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(30, ErrorMessage = "El teléfono no puede exceder los 30 caracteres")]
        public required string Numero { get; set; }

        [Required(ErrorMessage = "La etiqueta es obligatoria")]
        [MaxLength(50, ErrorMessage = "La etiqueta no puede exceder los 50 caracteres")]
        public required string Etiqueta { get; set; }

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }
    }
}
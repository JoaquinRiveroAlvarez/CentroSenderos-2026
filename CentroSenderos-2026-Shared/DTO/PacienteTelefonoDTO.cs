using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteTelefonoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(30, ErrorMessage = "El teléfono no puede exceder los 30 caracteres")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "La etiqueta es obligatoria")]
        [MaxLength(50, ErrorMessage = "La etiqueta no puede exceder los 50 caracteres")]
        public string Etiqueta { get; set; } = string.Empty;
    }
}
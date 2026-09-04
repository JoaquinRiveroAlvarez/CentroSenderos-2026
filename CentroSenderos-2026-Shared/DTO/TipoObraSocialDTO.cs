using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoObraSocialDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50,ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(50,ErrorMessage = "La descripción no puede exceder los 50 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [RegularExpression(@"^\d{2}-?\d{8}-?\d$",ErrorMessage = "El CUIT debe tener un formato válido")]
        public string Cuit { get; set; } = string.Empty;


        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}
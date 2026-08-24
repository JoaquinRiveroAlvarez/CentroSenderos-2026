using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ProfesionalDTO
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Ingresá el nombre completo.")]
        [MaxLength(100,ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public required string Nombre { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ingresá el área profesional.")]
        [MaxLength(100,ErrorMessage = "El área no puede superar los 100 caracteres.")]
        public required string Area { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ingresá el CUIT.")]
        [MaxLength(30, ErrorMessage = "El CUIT no puede superar los 30 caracteres.")]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$",ErrorMessage = "El CUIT debe tener el formato XX-XXXXXXXX-X.")]
        public required string Cuit { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ingresá la Matrícula Profesional.")]
        [MaxLength(30,ErrorMessage = "La Matrícula Profesional no puede superar los 30 caracteres.")]
        public required string MP { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ingresá el RNP.")]
        [MaxLength(30,ErrorMessage = "El RNP no puede superar los 30 caracteres.")]
        public required string RNP { get; set; } = string.Empty;


        [Required(ErrorMessage = "Ingresá el teléfono.")]
        [MaxLength(30,ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        [RegularExpression(@"^[0-9+\-\s()]+$", ErrorMessage = "El teléfono solo puede contener números, espacios, +, -, y paréntesis.")]
        public required string Telefono { get; set; } = string.Empty;


        public EnumEstadoRegistro EstadoRegistro { get; set; }

        public bool EsSocio { get; set; }

        [Required(ErrorMessage = "Ingresá el email.")]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        public required string Email { get; set; } = string.Empty;

        public string? RolAsignado { get; set; }
    }
}
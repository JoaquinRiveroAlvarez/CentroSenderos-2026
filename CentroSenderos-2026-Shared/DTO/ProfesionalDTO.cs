using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ProfesionalDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El Nombre no puede exceder los 100 caracteres")]
        public required string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Área es obligatoria")]
        [MaxLength(100, ErrorMessage = "El Área no puede exceder los 100 caracteres")]
        public required string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [MaxLength(30, ErrorMessage = "El CUIT no puede exceder los 20 caracteres")]
        public required string Cuit { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Matricula Profesional es obligatoria")]
        [MaxLength(30, ErrorMessage = "La Matricula Profesional no puede exceder los 20 caracteres")]
        public required string MP { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNP es obligatorio")]
        [MaxLength(30, ErrorMessage = "El RNP no puede exceder los 20 caracteres")]
        public required string RNP { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Teléfono es obligatorio")]
        [MaxLength(30, ErrorMessage = "El Teléfono no puede exceder los 20 caracteres")]
        public required string Telefono { get; set; } = string.Empty;
        public EnumEstadoRegistro EstadoRegistro { get; set; } = EnumEstadoRegistro.EnGrabacion;
    }
}

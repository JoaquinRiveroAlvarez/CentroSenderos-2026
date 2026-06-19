using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Tipo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Tipo no puede exceder los 50 caracteres")]
        public required string Tipo { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "La Descripción no puede exceder los 50 caracteres")]
        public string Descripcion { get; set; } = string.Empty;
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}

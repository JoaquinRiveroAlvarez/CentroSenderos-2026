using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoPlanillaCrearDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Nombre no puede exceder los 50 caracteres")]
        public required string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Tipo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Tipo no puede exceder los 50 caracteres")]
        public required string Tipo { get; set; } = string.Empty;

    }
}

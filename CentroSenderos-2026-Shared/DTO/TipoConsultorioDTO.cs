using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoConsultorioDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Tipo es obligatorio")]
        [MaxLength(100, ErrorMessage = "El Tipo no puede exceder los 100 caracteres")]
        public required string Tipo { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "La dirección no puede exceder los 100 caracteres")]
        public string Direccion { get; set; } = string.Empty;

        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}


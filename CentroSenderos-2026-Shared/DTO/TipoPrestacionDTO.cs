using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoPrestacionDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Tipo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Tipo no puede exceder los 50 caracteres")]
        public required string Tipo { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "La Descripción no puede exceder los 50 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Código es obligatorio")]
        [MaxLength(100, ErrorMessage = "El Código no puede exceder los 100 caracteres")]
        public required string Cod { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Monto por Sesion es obligatorio")]
        public required decimal MontoSesion { get; set; } = 0;
        public EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}

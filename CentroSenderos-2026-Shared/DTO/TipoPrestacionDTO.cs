using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoPrestacionDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Tipo es obligatorio")]
        [MaxLength(50, ErrorMessage = "El Tipo no puede exceder los 50 caracteres")]
        public required string Tipo { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "La Descripción no puede exceder los 200 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Código es obligatorio")]
        [MaxLength(100, ErrorMessage = "El Código no puede exceder los 100 caracteres")]
        public required string Cod { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Monto por Sesion es obligatorio")]
        public required decimal MontoSesion { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal PorcentajeRetencion { get; set; } = 20;

        public EnumEstadoRegistro EstadoRegistro { get; set; } = EnumEstadoRegistro.activo;
    }
}
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoDocumentoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de documento es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public required string Tipo { get; set; }

        [MaxLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
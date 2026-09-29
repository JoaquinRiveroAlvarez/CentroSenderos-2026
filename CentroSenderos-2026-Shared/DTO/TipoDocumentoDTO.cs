using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoDocumentoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de documento es obligatorio.")]
        public required string Tipo { get; set; }

        public string? Descripcion { get; set; }
    }
}
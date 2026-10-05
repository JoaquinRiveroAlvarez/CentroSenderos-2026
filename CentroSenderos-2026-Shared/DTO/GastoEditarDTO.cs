using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoEditarDTO : GastoCrearDTO
    {
        [Required(ErrorMessage = "Ingresá el motivo de la modificación.")]
        [MaxLength(500,
            ErrorMessage = "El motivo no puede exceder los 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;

        public bool CompletarReparto { get; set; }

        public List<int> SocioRepartoIds { get; set; } = new();


    }
}
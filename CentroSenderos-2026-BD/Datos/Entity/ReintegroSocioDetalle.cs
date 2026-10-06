using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class ReintegroSocioDetalle : EntityBase
    {
        [Required(ErrorMessage = "El reintegro es obligatorio")]
        public int ReintegroSocioId { get; set; }

        public ReintegroSocio? ReintegroSocio { get; set; }

        [Required(ErrorMessage = "El gasto es obligatorio")]
        public int GastoId { get; set; }

        public Gasto? Gasto { get; set; }

        [Required(ErrorMessage = "El monto aplicado es obligatorio")]
        public decimal Monto { get; set; }
    }
}
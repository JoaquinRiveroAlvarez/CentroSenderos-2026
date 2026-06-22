using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class GastoSocio : EntityBase
    {
        [Required(ErrorMessage = "El gasto es obligatorio")]
        public int GastoId { get; set; }
        public Gasto? Gastos { get; set; }

        [Required(ErrorMessage = "El socio es obligatorio")]
        public int SocioId { get; set; }
        public Socio? Socios { get; set; }
    }
}
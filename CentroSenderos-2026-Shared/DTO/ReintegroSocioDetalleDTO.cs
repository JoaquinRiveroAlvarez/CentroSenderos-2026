using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ReintegroSocioDetalleDTO
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione un gasto")]
        public int GastoId { get; set; }

        [Range(typeof(decimal),"0.01","9999999999999999.99",ParseLimitsInInvariantCulture = true,ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Monto { get; set; }
    }
}
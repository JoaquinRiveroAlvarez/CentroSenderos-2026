using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ReintegroSocioDTO
    {
        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione el socio que paga")]
        public int SocioPagadorId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccione el socio que recibe")]
        public int SocioReceptorId { get; set; }

        [Range(typeof(decimal),"0.01","9999999999999999.99",ParseLimitsInInvariantCulture = true,ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Monto { get; set; }

        public string Observacion { get; set; } = string.Empty;

        [MinLength(1,
            ErrorMessage = "Seleccione al menos un gasto")]
        public List<ReintegroSocioDetalleDTO> Detalles { get; set; } = new();
    }
}
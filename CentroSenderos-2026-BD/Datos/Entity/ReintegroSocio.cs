using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class ReintegroSocio : EntityBase
    {
        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El socio que paga es obligatorio")]
        public int SocioPagadorId { get; set; }

        public Socio? SocioPagador { get; set; }

        [Required(ErrorMessage = "El socio que recibe es obligatorio")]
        public int SocioReceptorId { get; set; }

        public Socio? SocioReceptor { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        public decimal Monto { get; set; }

        public List<ReintegroSocioDetalle> Detalles { get; set; } = new();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Liquidacion : EntityBase
    {
        [Required(ErrorMessage = "La Fecha Desde es obligatoria")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La Fecha Hasta es obligatoria")]
        public DateTime FechaHasta { get; set; }

        [Required(ErrorMessage = "El Número de Liquidación es obligatorio")]
        [MaxLength(20, ErrorMessage = "Máxima cantidad de caracteres: 20")]
        public string NumeroLiquidacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El socio es obligatorio")]
        public int SocioId { get; set; }
        public Socio? Socios { get; set; }

        [Required(ErrorMessage = "El profesional es obligatorio")]
       
        
        public int ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }

        public List<DetalleLiquidacion> DetalleLiquidaciones { get; set; } = new();
    }
}
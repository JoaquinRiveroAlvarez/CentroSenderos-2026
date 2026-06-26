using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class TurnoTipoPrestacion : EntityBase
    {
        [Required(ErrorMessage = "El turno es obligatorio")]
        public int TurnoId { get; set; }
        public Turno? Turnos { get; set; }

        [Required(ErrorMessage = "El tipo de prestación es obligatoria")]
        public int TipoPrestacionId { get; set; }
        public TipoPrestacion? TipoPrestaciones { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class TurnoTipoPrestacion : EntityBase
    {
        [Required(ErrorMessage = "El turno es obligatorio")]
        public int TurnoId { get; set; }

        public Turno? Turnos { get; set; }


        [Required(ErrorMessage = "El profesional es obligatorio")]
        public int ProfesionalId { get; set; }

        public Profesional? Profesionales { get; set; }


        [Required(ErrorMessage = "El tipo de prestación es obligatorio")]
        public int TipoPrestacionId { get; set; }

        public TipoPrestacion? TipoPrestaciones { get; set; }
    }
}
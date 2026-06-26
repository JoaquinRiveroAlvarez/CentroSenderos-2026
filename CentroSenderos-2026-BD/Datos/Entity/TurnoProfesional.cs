using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class TurnoProfesional : EntityBase
    {
        [Required(ErrorMessage = "El turno es obligatorio")]
        public int TurnoId { get; set; }
        public Turno? Turnos { get; set; }

        [Required(ErrorMessage = "El profesional es obligatorio")]
        public int ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }
    }
}

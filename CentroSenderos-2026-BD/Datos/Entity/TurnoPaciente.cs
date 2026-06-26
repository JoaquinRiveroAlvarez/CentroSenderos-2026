using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class TurnoPaciente : EntityBase
    {
        [Required(ErrorMessage = "El turno es obligatorio")]
        public int TurnoId { get; set; }
        public Turno? Turnos { get; set; }

        [Required(ErrorMessage = "El paciente es obligatorio")]
        public int PacienteId { get; set; }
        public Paciente? Pacientes { get; set; }
    }
}

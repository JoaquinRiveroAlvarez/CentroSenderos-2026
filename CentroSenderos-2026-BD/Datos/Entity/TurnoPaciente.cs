using System.ComponentModel.DataAnnotations;

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


        /*
          Obra social que tenía el paciente cuando se creó el turno.
          Es nullable para conservar los turnos existentes.
         */
        public int? TipoObraSocialId { get; set; }

        public TipoObraSocial? TipoObraSocial { get; set; }


        /*
          Copia histórica del nombre.
          Evita que un cambio de nombre en la obra social
          modifique la información de turnos anteriores.
         */
        public string? NombreObraSocial { get; set; }
    }
}
using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class SerieTurno : EntityBase
    {
        public EnumFrecuenciaRecurrenciaTurno Frecuencia { get; set; }
            = EnumFrecuenciaRecurrenciaTurno.semanal;

        [Range(
            1,
            365,
            ErrorMessage = "El intervalo debe ser mayor que cero."
        )]
        public int Intervalo { get; set; } = 1;

        public EnumUnidadRecurrenciaTurno? UnidadPersonalizada { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
        public DateTime FechaHasta { get; set; }

        public List<Turno> Turnos { get; set; } = new();
    }
}
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class DetalleLiquidacionTurno : EntityBase
    {
        [Required]
        public int DetalleLiquidacionId { get; set; }
        public DetalleLiquidacion? DetalleLiquidacion { get; set; }

        [Required]
        public int TurnoId { get; set; }
        public Turno? Turno { get; set; }

        // Para sesiones individuales/grupales vincula el paciente específico liquidado
        public int? PacienteId { get; set; }
        public Paciente? Paciente { get; set; }
    }
}
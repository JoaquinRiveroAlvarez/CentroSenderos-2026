using System;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class DetalleLiquidacion : EntityBase
    {
        [Required(ErrorMessage = "El monto de facturación es obligatorio")]
        public decimal MontoFacturacion { get; set; }

        [Required(ErrorMessage = "El porcentaje es obligatorio")]
        public decimal Porcentaje { get; set; }

        [Required(ErrorMessage = "La retención es obligatoria")]
        public decimal Retencion { get; set; }

        [Required(ErrorMessage = "La fecha del turno es obligatoria")]
        public DateTime FechaTurno { get; set; }

        [Required(ErrorMessage = "El paciente es obligatorio")]
        public int PacienteId { get; set; }
        public Paciente? Pacientes { get; set; }

        [Required(ErrorMessage = "El profesional es obligatorio")]
        public int ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }

        [Required(ErrorMessage = "La liquidación es obligatoria")]
        public int LiquidacionId { get; set; }
        public Liquidacion? Liquidaciones { get; set; }

        [Required(ErrorMessage = "La modalidad es obligatoria")]
        public int TipoModalidadId { get; set; }
        public TipoModalidad? TipoModalidades { get; set; }
    }
}
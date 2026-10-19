using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Turno : EntityBase
    {
        public EnumEstadoTurno EstadoTurno { get; set; } = EnumEstadoTurno.disponible;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        public DateTime FechaInicio { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        public DateTime FechaFin { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "El tipo de turno es obligatorio")]
        public int TipoTurnoId { get; set; }
        public TipoTurno? TipoTurnos { get; set; }

        [Required(ErrorMessage = "El tipo de consultorio es obligatorio")]
        public int TipoConsultorioId { get; set; }
        public TipoConsultorio? TipoConsultorios { get; set; }

        public int? SerieTurnoId { get; set; }
        public SerieTurno? SerieTurno { get; set; }

        public int? LiquidacionId { get; set; }
        public Liquidacion? Liquidacion { get; set; }

        public int? DetalleLiquidacionId { get; set; }
        public DetalleLiquidacion? DetalleLiquidacion { get; set; }

        public List<TurnoProfesional> TurnoProfesionales { get; set; } = new();
        public List<TurnoPaciente> TurnoPacientes { get; set; } = new();
        public List<TurnoTipoPrestacion> TurnoTipoPrestaciones { get; set; } = new();
    }
}


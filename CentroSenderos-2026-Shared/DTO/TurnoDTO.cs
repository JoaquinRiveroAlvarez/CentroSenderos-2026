using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
public class TurnoDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La hora es obligatoria")]
    public TimeOnly Hora { get; set; }

    public EnumEstadoTurno EstadoTurno { get; set; }

    public DateTime FechaInicio => Fecha.Date.Add(Hora.ToTimeSpan());
    public DateTime FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de turno válido")]
    public int TipoTurnoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un consultorio válido")]
    public int TipoConsultorioId { get; set; }

    public int DuracionPersonalizada { get; set; } = 0;
    // Configuración de recurrencia.
    public EnumFrecuenciaRecurrenciaTurno FrecuenciaRecurrencia { get; set; }
        = EnumFrecuenciaRecurrenciaTurno.noRepite;

    public DateTime? FechaHastaRecurrencia { get; set; }

    public int IntervaloRecurrencia { get; set; } = 1;

    public EnumUnidadRecurrenciaTurno? UnidadRecurrencia { get; set; }

    // Se completa cuando el turno pertenece a una serie existente.
    public int? SerieTurnoId { get; set; }




    // Propiedades anteriores.
    // Se mantienen temporalmente mientras adaptamos el repositorio y el frontend.
    public int ProfesionalId { get; set; }
    public int PacienteId { get; set; }

    public string? NombreProfesional { get; set; }
    public string? NombrePaciente { get; set; }

    // Nuevas propiedades para permitir múltiples profesionales y pacientes.
    public List<int> ProfesionalIds { get; set; } = new();
    public List<int> PacienteIds { get; set; } = new();
}


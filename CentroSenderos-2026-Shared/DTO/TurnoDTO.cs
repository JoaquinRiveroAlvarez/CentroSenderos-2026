using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;
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
}

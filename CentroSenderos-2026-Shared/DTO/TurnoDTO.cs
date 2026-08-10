using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

public class TurnoDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "La hora es obligatoria")]
    public TimeOnly Hora { get; set; }

    public DateTime FechaInicio => Fecha.ToDateTime(Hora);

    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El estado del turno es obligatorio")]
    public EnumEstadoTurno EstadoTurno { get; set; }

    [Required(ErrorMessage = "El tipo de turno es obligatorio")]
    public int TipoTurnoId { get; set; }

    [Required(ErrorMessage = "El consultorio es obligatorio")]
    public int TipoConsultorioId { get; set; }

    public int DuracionPersonalizada { get; set; } = 0;
}

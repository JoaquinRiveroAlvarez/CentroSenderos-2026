using CentroSenderos_2026_Shared.Enum;
using System.ComponentModel.DataAnnotations;

public class TurnoDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    [CustomValidation(typeof(TurnoDTO), nameof(ValidarFecha))]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La hora es obligatoria")]
    public TimeOnly Hora { get; set; }

    public DateTime FechaInicio => Fecha.ToDateTime(Hora);
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El estado del turno es obligatorio")]
    public EnumEstadoTurno EstadoTurno { get; set; } = EnumEstadoTurno.reservado;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de turno válido")]
    public int TipoTurnoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un consultorio válido")]
    public int TipoConsultorioId { get; set; }

    public int DuracionPersonalizada { get; set; } = 0;

    // Validadores personalizados
    public static ValidationResult? ValidarFecha(DateOnly fecha, ValidationContext context)
    {
        return fecha == DateOnly.MinValue
            ? new ValidationResult("Debe seleccionar una fecha válida")
            : ValidationResult.Success;
    }
}

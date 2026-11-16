namespace CentroSenderos_2026_Shared.DTO;

public class HorarioTurnoDTO
{
    public string Hora { get; set; } = string.Empty;
    public List<ConflictoTurnoDTO> Conflictos { get; set; } = new();
    public bool Ocupado => Conflictos.Count > 0;
}

public class ConflictoTurnoDTO
{
    public bool ConflictoConsultorio { get; set; }
    public bool ConflictoProfesional { get; set; }
    public bool ConflictoPaciente { get; set; }
    public IEnumerable<string> Motivos => new[]
    {
        ConflictoConsultorio ? "Consultorio ocupado" : null,
        ConflictoProfesional ? "Profesional con otro turno" : null,
        ConflictoPaciente ? "Paciente con otro turno" : null
    }.OfType<string>();

    public int TurnoId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fin { get; set; }
    public string Consultorio { get; set; } = string.Empty;
    public List<string> Profesionales { get; set; } = new();
    public List<string> Pacientes { get; set; } = new();
}

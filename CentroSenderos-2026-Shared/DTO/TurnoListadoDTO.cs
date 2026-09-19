using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TurnoListadoDTO
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EnumEstadoTurno EstadoTurno { get; set; }

        public int TipoTurnoId { get; set; }
        public string? NombreTipoTurno { get; set; }

        public int TipoConsultorioId { get; set; }
        public string? NombreTipoConsultorio { get; set; }

        // Se conservan para compatibilidad con las vistas anteriores.
        public List<int> TipoPrestacionIds { get; set; } = new();
        public List<string> NombresTipoPrestaciones { get; set; } = new();

        public string? NombreTipoPrestacion =>
            NombresTipoPrestaciones.Count == 0
                ? null
                : string.Join(", ", NombresTipoPrestaciones);

        // Mantiene unidas la prestación y la persona que la realiza.
        public List<TurnoProfesionalPrestacionDTO>
            ProfesionalPrestaciones
        { get; set; } = new();

        public int? SerieTurnoId { get; set; }

        public EnumFrecuenciaRecurrenciaTurno FrecuenciaRecurrencia { get; set; }
            = EnumFrecuenciaRecurrenciaTurno.noRepite;

        public int IntervaloRecurrencia { get; set; } = 1;

        public EnumUnidadRecurrenciaTurno? UnidadRecurrencia { get; set; }

        public DateTime? FechaHastaRecurrencia { get; set; }

        // Compatibilidad temporal con las propiedades individuales.
        public int ProfesionalId { get; set; }
        public string? NombreProfesional { get; set; }

        public int PacienteId { get; set; }
        public string? NombrePaciente { get; set; }

        public List<int> ProfesionalIds { get; set; } = new();
        public List<string> NombresProfesionales { get; set; } = new();

        public List<int> PacienteIds { get; set; } = new();
        public List<string> NombresPacientes { get; set; } = new();

        public List<TurnoPacienteDetalleDTO> PacientesDetalle { get; set; } = new();
    }
}

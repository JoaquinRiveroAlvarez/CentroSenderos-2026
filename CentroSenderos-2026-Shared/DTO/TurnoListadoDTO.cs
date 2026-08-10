using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}


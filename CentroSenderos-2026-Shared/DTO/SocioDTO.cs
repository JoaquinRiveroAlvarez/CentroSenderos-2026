using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class SocioDTO
    {
        public int Id { get; set; }
        public int ProfesionalId { get; set; }
        public string Profesional { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public EnumEstadoRegistro EstadoRegistro { get; set; } = EnumEstadoRegistro.activo;
    }

}

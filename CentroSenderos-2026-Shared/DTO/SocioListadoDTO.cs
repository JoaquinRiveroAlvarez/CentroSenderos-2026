using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class SocioListadoDTO
    {
        public int Id { get; set; }
        public int ProfesionalId { get; set; }
        public string Profesional { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
    }

}

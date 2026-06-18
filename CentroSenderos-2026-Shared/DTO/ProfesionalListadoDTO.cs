using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class ProfesionalListadoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Cuit { get ; set; } = string.Empty;
        public string MP { get; set; } = string.Empty;
        public string RNP { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}

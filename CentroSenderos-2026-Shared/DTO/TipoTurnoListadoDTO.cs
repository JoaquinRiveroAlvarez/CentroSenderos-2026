using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoTurnoListadoDTO
    {
        public int Id { get; set; }
        public required string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public required int DuracionMinutos { get; set; } = 0;
    }
}

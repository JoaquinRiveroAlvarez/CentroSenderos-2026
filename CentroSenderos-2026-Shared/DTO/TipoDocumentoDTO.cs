using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoDocumentoDTO
    {
        public int Id { get; set; }        
        public required string Tipo { get; set; }
        public string? Descripcion { get; set; } 
    }
}


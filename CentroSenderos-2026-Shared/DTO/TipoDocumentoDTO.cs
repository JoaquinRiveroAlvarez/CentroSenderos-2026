using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class TipoDocumentoDTO
    {
        public int Id { get; set; }        // Identificador del tipo de documento
        public required string Tipo { get; set; }  // Nombre o descripción (ej: DNI, Pasaporte, etc.)
        public string? Descripcion { get; set; } // Opcional: detalle adicional
    }
}


using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Shared.DTO
{
    public class DocumentoDTO
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int TipoDocumentoId { get; set; }
        public string TipoDocumentoNombre { get; set; } = string.Empty;
        public string UrlArchivo { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }

        // Nombre dinámico construido en el repositorio
        public string NombreGenerado { get; set; } = string.Empty;
    }

}

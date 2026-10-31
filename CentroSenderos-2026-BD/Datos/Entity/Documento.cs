using System;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Documento : EntityBase
    {
        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        public int TipoDocumentoId { get; set; }
        public TipoDocumento? TipoDocumentos { get; set; }

        [Required]
        public int PacienteId { get; set; }
        public Paciente? Paciente { get; set; }

        public string UrlArchivo { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        public string NombreGenerado { get; set; } = string.Empty;

        // Campos para almacenamiento en Neon PostgreSQL
        public string NombreOriginal { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public long TamanoBytes { get; set; }
    }
}
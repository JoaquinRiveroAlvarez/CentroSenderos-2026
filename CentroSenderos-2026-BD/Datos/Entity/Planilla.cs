using System;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Planilla : EntityBase
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;
        public int TipoPlanillaId { get; set; }
        public TipoPlanilla? TipoPlanilla { get; set; }
        [MaxLength(255)]
        public string NombreOriginal { get; set; } = string.Empty;
        [MaxLength(255)]
        public string NombreGenerado { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    }
}
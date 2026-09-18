using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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

        [Required]
        public string UrlArchivo { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        public string NombreGenerado { get; set; } = string.Empty;
    }

}

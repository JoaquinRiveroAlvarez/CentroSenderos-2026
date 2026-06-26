using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(DNI), Name = "DNI_UQ", IsUnique = true)]
    [Index(nameof(NumeroAfiliado), Name = "NumeroAfiliado_UQ", IsUnique = true)]
    public class Paciente : EntityBase
    {
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "Maxima cant. caracteres 50")]
        public required string Nombre {  get; set; }

        [Required(ErrorMessage ="El DNI es obligatorio")]
        [MaxLength(10, ErrorMessage ="Maxima cant. caracteres 10")]
        public required string DNI { get; set; }


        [Required(ErrorMessage = "El NroAfiliado es obligatorio")]
        public required int NumeroAfiliado { get; set; }

        [Required(ErrorMessage = "El Telefono es obligatorio")]
        [MaxLength(30, ErrorMessage = "Maxima cant. caracteres 30")]
        public required string Telefono { get; set; }

        [Required(ErrorMessage = "El Domicilio es obligatorio")]
        [MaxLength(30, ErrorMessage = "Maxima cant. caracteres 30")]
        public required string Domicilio { get; set; }

        [Required(ErrorMessage = "La obra social es obligatoria")]
        public int TipoObraSocialId { get; set; }
        public TipoObraSocial? TipoObraSociales { get; set; }

        public int TipoDiagnosticoId { get; set; }
        public TipoDiagnostico? TipoDiagnosticos { get; set; }

        public int DocumentoId { get; set; }
        public Documento? Documentos { get; set; }

        public List<TurnoPaciente> TurnoPacientes { get; set; } = new();

        public List<DetalleLiquidacion> DetalleLiquidaciones { get; set; } = new();
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(DNI), Name = "DNI_UQ", IsUnique = true)]
    public class Paciente : EntityBase
    {
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "Maxima cant. caracteres 50")]
        public required string Nombre {  get; set; }

        [Required(ErrorMessage ="El DNI es obligatorio")]
        [MaxLength(10, ErrorMessage ="Maxima cant. caracteres 10")]
        public required string DNI { get; set; }

        [Required(ErrorMessage ="La Fecha de Nacimiento es obligatoria")]
        public required DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El NroAfiliado es obligatorio")]
        [MaxLength(30, ErrorMessage = "Maxima cant. caracteres 30")]
        public required int NroAfiliado { get; set; }

        [Required(ErrorMessage = "El Telefono es obligatorio")]
        [MaxLength(30, ErrorMessage = "Maxima cant. caracteres 30")]
        public required int Telefono { get; set; }

        [Required(ErrorMessage = "El Domicilio es obligatorio")]
        [MaxLength(30, ErrorMessage = "Maxima cant. caracteres 30")]
        public required string Domicilio { get; set; }


        //fk
        public int IdObraSocial { get; set; }
        public int IdDiagnostico { get; set; }
    }
}

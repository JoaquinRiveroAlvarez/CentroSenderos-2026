using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class ObraSocial : EntityBase
    {
        [Required(ErrorMessage = "El Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "Maxima cant. caracteres 50")]
        public required string Nombre {  get; set; }


        // Relación inversa con Pacientes
        public ICollection<Paciente> Pacientes { get; set; }
    }
}

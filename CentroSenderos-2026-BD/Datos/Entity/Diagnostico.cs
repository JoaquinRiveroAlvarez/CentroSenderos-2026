using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Diagnostico : EntityBase
    {
        [Required(ErrorMessage = "El Tipo es obligatorio")]
        public required string Tipo { get; set; }


        // Relación inversa con Pacientes
        public ICollection<Paciente> Pacientes { get; set; }
    }
}

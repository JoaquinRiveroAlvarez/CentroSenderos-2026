using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Gasto : EntityBase
    {
        [Required(ErrorMessage = "La Fecha es obligatoria")]
        public required DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La Descripcion es obligatoria")]
        [MaxLength(100, ErrorMessage = "Maxima cant. caracteres 100")]
        public required string Descripcion { get; set; }

        [Required(ErrorMessage = "El Monto es obligatorio")] 
        public required decimal Monto { get; set; }

        [Required(ErrorMessage = "El tipo de gasto es obligatorio")]
        public int TipoGastoId { get; set; }
        public TipoGasto? TipoGastos { get; set; }


        public List<GastoSocio> GastoSocios { get; set; } = new();
    }
}

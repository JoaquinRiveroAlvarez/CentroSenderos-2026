using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoPrestacion_Tipo_UQ", IsUnique = true)]
    public class TipoPrestacion : EntityTipoBase
    {
        [Required(ErrorMessage = "El Código es obligatorio")]
        [MaxLength(100, ErrorMessage = "El Código no puede exceder los 100 caracteres")]
        public required string Cod { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Monto por Sesion es obligatorio")]
        public required decimal MontoSesion { get; set; } = 0;
    }
}

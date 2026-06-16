using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoConsultorio_Tipo_UQ", IsUnique = true)]
    public class TipoConsultorio : EntityTipoBase
    {
        [MaxLength(100, ErrorMessage = "La dirección no puede exceder los 100 caracteres")]
        public string Direccion { get; set; } = string.Empty;
    }
}

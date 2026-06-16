using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoTurno_Tipo_UQ", IsUnique = true)]
    public class TipoTurno : EntityTipoBase
    {
        [Required(ErrorMessage = "La duración en minutos es obligatoria")]
        public required int DuracionMinutos { get; set; }
    }
}

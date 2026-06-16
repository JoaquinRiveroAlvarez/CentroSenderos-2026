using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoPlanilla_Tipo_UQ", IsUnique = true)]
    public class TipoPlanilla : EntityTipoBase
    {
    }
}

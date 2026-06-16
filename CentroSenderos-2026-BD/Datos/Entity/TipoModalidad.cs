using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoModalidad_Tipo_UQ", IsUnique = true)]
    public class TipoModalidad : EntityTipoBase
    {
    }
}

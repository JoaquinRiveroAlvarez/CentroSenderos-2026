using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoDocumento_Tipo_UQ", IsUnique = true)]
    public class TipoDocumento : EntityTipoBase
    {

    }
}

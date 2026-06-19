using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoObraSocial_Tipo_UQ", IsUnique = true)]
    public class TipoObraSocial : EntityTipoBase
    {
       
    }
}

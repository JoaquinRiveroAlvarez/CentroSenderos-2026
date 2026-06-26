using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CentroSenderos_2026_BD.Datos.EntityTipoBase;

namespace CentroSenderos_2026_BD.Datos
{
    public class EntityTipoBase : EntityBase
    {
        public required string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}

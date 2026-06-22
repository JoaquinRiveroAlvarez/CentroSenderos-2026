using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoGasto_Tipo_UQ", IsUnique = true)]
    public class TipoGasto : EntityTipoBase
    {
        public List<Gasto> Gastos { get; set; } = new();
    }
}

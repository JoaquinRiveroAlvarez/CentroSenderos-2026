using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CentroSenderos_2026_BD.Datos.EntityBase;

namespace CentroSenderos_2026_BD.Datos
{
    public class EntityBase : IEntityBase
    {
        public int Id { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public EnumEstadoRegistro EstadoRegistro { get; set; } = EnumEstadoRegistro.EnGrabacion;
    }
}

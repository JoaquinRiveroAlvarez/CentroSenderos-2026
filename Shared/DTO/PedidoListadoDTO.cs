using Modelado2025_1Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelado2025_1Shared.DTO
{
    public class PedidoListadoDTO
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public MetodoDePago MetodoDePago { get; set; }
        public List<DetallePedidoListadoDTO> Detalles { get; set; } = new();
        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}

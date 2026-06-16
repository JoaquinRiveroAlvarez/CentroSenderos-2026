using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelado2025_1Shared.DTO
{
    public class PedidoResumenDTO
    {
        public int Id { get; set; }
        public string Pedido { get; set; } = string.Empty;
        public List<string> DetallesCadaPedido { get; set; } = new List<string>();
    }
}

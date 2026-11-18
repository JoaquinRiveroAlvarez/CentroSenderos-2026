using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/caja")]
    public class CajaController(ApplicationDbContext contexto) : ControllerBase
    {
        private const string Cobro = "Cobro de liquidación";
        private const string Anulacion = "Anulación de cobro";
        private static readonly TimeZoneInfo ZonaArgentina =
            TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");

        [HttpGet("movimientos")]
        public async Task<ActionResult<List<MovimientoCajaDTO>>> Movimientos()
        {
            var cobros = contexto.PagosLiquidaciones
                .Where(pago => pago.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(pago => new MovimientoCajaDTO
                {
                    OrigenId = pago.Id,
                    Fecha = pago.FechaPago,
                    Concepto = Cobro,
                    Referencia = pago.Liquidacion!.NumeroLiquidacion,
                    Importe = pago.MontoTotal
                });

            var anulaciones = contexto.PagosLiquidaciones
                .Where(pago => pago.EstadoRegistro == EnumEstadoRegistro.activo &&
                    pago.Anulado && pago.FechaAnulacion.HasValue)
                .Select(pago => new MovimientoCajaDTO
                {
                    OrigenId = pago.Id,
                    Fecha = pago.FechaAnulacion!.Value,
                    Concepto = Anulacion,
                    Referencia = pago.Liquidacion!.NumeroLiquidacion,
                    Importe = -pago.MontoTotal
                });

            var pagosDirectos = contexto.Gastos
                .Where(gasto => gasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .SelectMany(gasto => gasto.GastoSocios
                    .Where(aporte => aporte.EstadoRegistro == EnumEstadoRegistro.activo &&
                        aporte.Socios!.EsCaja)
                    .Select(aporte => new MovimientoCajaDTO
                    {
                        OrigenId = gasto.Id,
                        Fecha = gasto.Fecha,
                        Concepto = "Pago directo de Caja",
                        Referencia = gasto.Descripcion,
                        Importe = -aporte.Monto
                    }));

            var reintegros = contexto.ReintegrosSocios
                .Where(reintegro => reintegro.EstadoRegistro == EnumEstadoRegistro.activo &&
                    reintegro.SocioPagador!.EsCaja)
                .Select(reintegro => new MovimientoCajaDTO
                {
                    OrigenId = reintegro.Id,
                    Fecha = reintegro.Fecha,
                    Concepto = "Reintegro a socio",
                    Referencia = reintegro.SocioReceptor!.Profesionales!.Nombre,
                    Importe = -reintegro.Monto
                });

            var movimientos = await cobros.Concat(anulaciones)
                .Concat(pagosDirectos).Concat(reintegros)
                .OrderByDescending(movimiento => movimiento.Fecha)
                .ThenBy(movimiento => movimiento.Concepto)
                .ThenByDescending(movimiento => movimiento.OrigenId)
                .ToListAsync();

            foreach (var movimiento in movimientos)
            {
                if (movimiento.Concepto is Cobro or Anulacion)
                    movimiento.Fecha = TimeZoneInfo.ConvertTimeFromUtc(movimiento.Fecha, ZonaArgentina);
            }

            return Ok(movimientos.OrderByDescending(movimiento => movimiento.Fecha).ToList());
        }
    }
}

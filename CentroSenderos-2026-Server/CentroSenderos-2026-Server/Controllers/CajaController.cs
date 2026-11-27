using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Authorize(Policy = "EsSocio")]
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
            await using var lectura = await contexto.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
            var cobros = contexto.PagosLiquidaciones
                .Where(pago => pago.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(pago => new MovimientoCajaDTO
                {
                    Tipo = TipoMovimientoCaja.Cobro,
                    OrigenId = pago.Id,
                    LiquidacionId = pago.LiquidacionId,
                    Fecha = pago.FechaPago,
                    Concepto = Cobro,
                    Referencia = pago.Liquidacion!.NumeroLiquidacion,
                    Importe = pago.MontoTotal
                });

            var anulaciones = contexto.PagosLiquidaciones
                .Where(pago => pago.EstadoRegistro == EnumEstadoRegistro.activo &&
                    ((pago.Anulado && pago.FechaAnulacion.HasValue) ||
                     (pago.Modificado && pago.FechaModificacion.HasValue)))
                .Select(pago => new MovimientoCajaDTO
                {
                    Tipo = TipoMovimientoCaja.Anulacion,
                    OrigenId = pago.Id,
                    LiquidacionId = pago.LiquidacionId,
                    Fecha = pago.Anulado && pago.FechaAnulacion.HasValue
                        ? pago.FechaAnulacion.Value
                        : pago.FechaModificacion!.Value,
                    Concepto = pago.Anulado ? Anulacion : "Modificación de cobro",
                    Referencia = pago.Liquidacion!.NumeroLiquidacion,
                    Importe = -pago.MontoTotal
                });

            var pagosDirectos = contexto.Gastos
                .Where(gasto => gasto.EstadoRegistro == EnumEstadoRegistro.activo || gasto.EstadoRegistro == EnumEstadoRegistro.anulado)
                .SelectMany(gasto => gasto.GastoSocios
                    .Where(aporte => aporte.EstadoRegistro == EnumEstadoRegistro.activo &&
                        aporte.Socios!.EsCaja)
                    .Select(aporte => new MovimientoCajaDTO
                    {
                        Tipo = TipoMovimientoCaja.PagoGasto,
                        OrigenId = gasto.Id,
                        LiquidacionId = 0,
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
                    Tipo = TipoMovimientoCaja.Reintegro,
                    OrigenId = reintegro.Id,
                    LiquidacionId = 0,
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

            var reversiones = await (
                from gasto in contexto.Gastos
                where gasto.EstadoRegistro == EnumEstadoRegistro.anulado
                from aporte in gasto.GastoSocios
                where aporte.EstadoRegistro == EnumEstadoRegistro.activo && aporte.Socios!.EsCaja
                from evento in contexto.GastoHistoriales.Where(h => h.GastoId == gasto.Id)
                    .OrderByDescending(h => h.FechaCambio).ThenByDescending(h => h.Id).Take(1)
                select new MovimientoCajaDTO
                {
                    Tipo = TipoMovimientoCaja.AnulacionGasto,
                    OrigenId = gasto.Id,
                    Fecha = evento.FechaCambio,
                    Concepto = "Anulación de gasto",
                    Referencia = gasto.Descripcion,
                    Importe = aporte.Monto
                }).ToListAsync();
            movimientos.AddRange(reversiones);
            var reintegrosRevertidos = await (
                from detalle in contexto.ReintegrosSociosDetalles
                where detalle.EstadoRegistro == EnumEstadoRegistro.anulado &&
                    detalle.Gasto!.EstadoRegistro == EnumEstadoRegistro.anulado &&
                    detalle.ReintegroSocio!.SocioPagador!.EsCaja
                from evento in contexto.GastoHistoriales.Where(h => h.GastoId == detalle.GastoId)
                    .OrderByDescending(h => h.FechaCambio).ThenByDescending(h => h.Id).Take(1)
                select new { detalle.GastoId, detalle.ReintegroSocioId, detalle.Monto, evento.FechaCambio })
                .ToListAsync();
            movimientos.AddRange(reintegrosRevertidos
                .GroupBy(r => new { r.GastoId, r.ReintegroSocioId, r.FechaCambio })
                .Select(grupo => new MovimientoCajaDTO
                {
                    Tipo = TipoMovimientoCaja.ReversionReintegro,
                    OrigenId = grupo.Key.GastoId,
                    Fecha = grupo.Key.FechaCambio,
                    Concepto = "Reversión de reintegro",
                    Referencia = $"Reintegro #{grupo.Key.ReintegroSocioId} por anulación del gasto #{grupo.Key.GastoId}",
                    Importe = grupo.Sum(r => r.Monto)
                }));
            await lectura.CommitAsync();

            foreach (var movimiento in movimientos)
            {
                if (movimiento.Tipo is TipoMovimientoCaja.Cobro or TipoMovimientoCaja.Anulacion or TipoMovimientoCaja.AnulacionGasto or TipoMovimientoCaja.ReversionReintegro)
                    movimiento.Fecha = TimeZoneInfo.ConvertTimeFromUtc(movimiento.Fecha, ZonaArgentina);
            }

            return Ok(movimientos.OrderByDescending(movimiento => movimiento.Fecha).ToList());
        }

        [HttpGet("reintegros/{id:int}/gastos")]
        public async Task<ActionResult<List<GastoMovimientoCajaDTO>>> GastosDelReintegro(int id)
        {
            var gastos = await contexto.ReintegrosSociosDetalles
                .Where(detalle => detalle.ReintegroSocioId == id &&
                    (detalle.EstadoRegistro == EnumEstadoRegistro.activo || detalle.EstadoRegistro == EnumEstadoRegistro.anulado) &&
                    detalle.ReintegroSocio!.EstadoRegistro == EnumEstadoRegistro.activo &&
                    detalle.ReintegroSocio.SocioPagador!.EsCaja)
                .OrderBy(detalle => detalle.GastoId)
                .Select(detalle => new GastoMovimientoCajaDTO
                {
                    GastoId = detalle.GastoId,
                    Revertido = detalle.EstadoRegistro == EnumEstadoRegistro.anulado,
                    Descripcion = detalle.Gasto!.Descripcion,
                    Monto = detalle.Monto
                }).ToListAsync();

            return Ok(gastos);
        }
    }
}
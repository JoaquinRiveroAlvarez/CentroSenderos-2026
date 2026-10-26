using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LiquidacionController : ControllerBase
    {
        private readonly ILiquidacionRepositorio liquidacionRepositorio;

        public LiquidacionController(ILiquidacionRepositorio liquidacionRepositorio)
        {
            this.liquidacionRepositorio = liquidacionRepositorio;
        }

        // GET: api/liquidacion?fechaDesde=2026-10-01&fechaHasta=2026-10-31&profesionalId=5
        [HttpGet]
        public async Task<ActionResult<List<LiquidacionResumenDTO>>> ObtenerLiquidaciones(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int? profesionalId)
        {
            var lista = await liquidacionRepositorio.ObtenerResumenLiquidaciones(fechaDesde, fechaHasta, profesionalId);
            return Ok(lista);
        }

        // GET: api/liquidacion/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LiquidacionDTO>> ObtenerPorId(int id)
        {
            var liquidacion = await liquidacionRepositorio.ObtenerPorIdCompleto(id);
            if (liquidacion == null)
                return NotFound($"No se encontró la liquidación con Id {id}");

            return Ok(liquidacion);
        }

        // GET: api/liquidacion/por-numero/LIQ-2718811
        [HttpGet("por-numero/{numeroLiquidacion}")]
        public async Task<ActionResult<LiquidacionDTO>> ObtenerPorNumero(string numeroLiquidacion)
        {
            var liquidacion = await liquidacionRepositorio.ObtenerPorNumero(numeroLiquidacion);
            if (liquidacion == null)
                return NotFound($"No se encontró la liquidación con Número {numeroLiquidacion}");

            return Ok(liquidacion);
        }

        // GET: api/liquidacion/detalle/{detalleId}/turnos
        // Retorna los turnos de la fila del detalle (Modal de sesiones de la factura)
        [HttpGet("detalle/{detalleId:int}/turnos")]
        public async Task<ActionResult<List<TurnoLiquidacionItemDTO>>> ObtenerTurnosDeDetalle(int detalleId)
        {
            var turnos = await liquidacionRepositorio.ObtenerTurnosPorDetalleId(detalleId);
            return Ok(turnos);
        }

        // GET: api/liquidacion/turnos-pendientes?profesionalId=5&fechaDesde=2026-10-01&fechaHasta=2026-10-31
        [HttpGet("turnos-pendientes")]
        public async Task<ActionResult<List<TurnoLiquidacionItemDTO>>> GetTurnosPendientes(
            [FromQuery] int? profesionalId,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta)
        {
            try
            {
                var lista = await liquidacionRepositorio.ObtenerTurnosPendientes(profesionalId, fechaDesde, fechaHasta);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // POST: api/liquidacion
        [HttpPost]
        public async Task<ActionResult<int>> Guardar([FromBody] GuardarLiquidacionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await liquidacionRepositorio.GuardarLiquidacionCompleta(dto);
            return Ok(id);
        }

        // PUT: api/liquidacion/5/estado-pago?nuevoEstado=4
        [HttpPut("{id:int}/estado-pago")]
        public async Task<IActionResult> ActualizarEstadoPagoLiquidacion(int id, [FromQuery] EnumEstadoPagoLiquidacion nuevoEstado)
        {
            var resultado = await liquidacionRepositorio.ActualizarEstadoPagoLiquidacion(id, nuevoEstado);
            if (!resultado)
                return NotFound();

            return NoContent();
        }

        // PUT: api/liquidacion/detalle/12/estado-pago?nuevoEstado=3
        [HttpPut("detalle/{detalleId:int}/estado-pago")]
        public async Task<IActionResult> ActualizarEstadoPagoDetalle(int detalleId, [FromQuery] EnumEstadoPagoLiquidacion nuevoEstado)
        {
            var resultado = await liquidacionRepositorio.ActualizarEstadoPagoDetalle(detalleId, nuevoEstado);
            if (!resultado)
                return NotFound();

            return NoContent();
        }

        // PUT: api/liquidacion/5/anular
        [HttpPut("{id:int}/anular")]
        public async Task<IActionResult> AnularLiquidacion(int id)
        {
            var liq = await liquidacionRepositorio.ObtenerPorIdCompleto(id);
            if (liq == null) return NotFound("No se encontró la liquidación.");

            // Regla de negocio: Si ya tiene pagos registrados (Parcial o Pagado), no se puede anular
            if (liq.EstadoPago == EnumEstadoPagoLiquidacion.Parcial || liq.EstadoPago == EnumEstadoPagoLiquidacion.Pagado)
            {
                return BadRequest("No se puede anular ni eliminar una liquidación que ya registra pagos.");
            }

            var resultado = await liquidacionRepositorio.AnularLiquidacion(id);
            if (!resultado) return BadRequest("No se pudo anular la liquidación.");

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Server.Controllers
{
    [Authorize(Policy = "EsSocio")]
    [ApiController]
    [Route("api/[controller]")]
    public class LiquidacionController : ControllerBase
    {
        private readonly ILiquidacionRepositorio liquidacionRepositorio;

        public LiquidacionController(ILiquidacionRepositorio liquidacionRepositorio)
        {
            this.liquidacionRepositorio = liquidacionRepositorio;
        }

        private string ObtenerUsuarioActual()
        {
            return User?.Identity?.Name
                ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? "Usuario";
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
        [HttpGet("detalle/{detalleId:int}/turnos")]
        public async Task<ActionResult<List<TurnoLiquidacionItemDTO>>> ObtenerTurnosDeDetalle(int detalleId)
        {
            var turnos = await liquidacionRepositorio.ObtenerTurnosPorDetalleId(detalleId);
            return Ok(turnos);
        }

        // POST: api/liquidacion/turnos-por-ids?pacienteId=1&profesionalId=2
        [HttpPost("turnos-por-ids")]
        public async Task<ActionResult<List<TurnoLiquidacionItemDTO>>> ObtenerTurnosPorIds(
            [FromBody] List<int> turnoIds,
            [FromQuery] int? pacienteId,
            [FromQuery] int? profesionalId)
        {
            var turnos = await liquidacionRepositorio.ObtenerTurnosPorIds(turnoIds, pacienteId, profesionalId);
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

        // POST: api/liquidacion/validar-detalle-manual
        [HttpPost("validar-detalle-manual")]
        public async Task<ActionResult<ValidarDetalleManualResponseDTO>> ValidarDetalleManual([FromBody] ValidarDetalleManualRequestDTO request)
        {
            try
            {
                var resultado = await liquidacionRepositorio.ValidarDetalleManualContraAgenda(request);
                return Ok(resultado);
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

            if (string.IsNullOrWhiteSpace(dto.UsuarioResponsable))
                dto.UsuarioResponsable = ObtenerUsuarioActual();

            try
            {
                var id = await liquidacionRepositorio.GuardarLiquidacionCompleta(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al guardar la liquidación: " + ex.Message });
            }
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

            if (liq.EstadoPago == EnumEstadoPagoLiquidacion.Parcial || liq.EstadoPago == EnumEstadoPagoLiquidacion.Pagado)
            {
                return BadRequest("No se puede anular una liquidación que ya registra cobros.");
            }

            var resultado = await liquidacionRepositorio.AnularLiquidacion(id);
            if (!resultado) return BadRequest("No se pudo anular la liquidación.");

            return NoContent();
        }

        // DELETE: api/liquidacion/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarLiquidacion(int id)
        {
            var liq = await liquidacionRepositorio.ObtenerPorIdCompleto(id);
            if (liq == null) return NotFound("No se encontró la liquidación a eliminar.");

            if (liq.EstadoPago == EnumEstadoPagoLiquidacion.Parcial || liq.EstadoPago == EnumEstadoPagoLiquidacion.Pagado)
            {
                return BadRequest("No se puede eliminar una liquidación que registra cobros.");
            }

            var resultado = await liquidacionRepositorio.EliminarLiquidacion(id);
            if (!resultado) return BadRequest("No se pudo eliminar la liquidación.");

            return NoContent();
        }

        // ==========================================
        // HISTORIAL DE PAGOS Y COBROS
        // ==========================================

        // GET: api/liquidacion/5/pagos
        [HttpGet("{id:int}/pagos")]
        public async Task<ActionResult<List<PagoLiquidacionHistorialDTO>>> ObtenerHistorialPagos(int id)
        {
            try
            {
                var pagos = await liquidacionRepositorio.ObtenerHistorialPagos(id);
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener historial de cobros: " + ex.Message });
            }
        }

        // POST: api/liquidacion/registrar-cobro
        [HttpPost("registrar-cobro")]
        public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarCobroLiquidacionDTO dto)
        {
            if (dto.LiquidacionId <= 0 || dto.Metodos == null || dto.Metodos.Count == 0 || dto.Imputaciones == null || dto.Imputaciones.Count == 0)
            {
                return BadRequest("Debe especificar la liquidación, al menos un método de pago y al menos un detalle imputado.");
            }

            if (string.IsNullOrWhiteSpace(dto.RegistradoPor))
                dto.RegistradoPor = ObtenerUsuarioActual();

            decimal totalMetodos = dto.Metodos.Sum(m => m.Monto);
            decimal totalImputado = dto.Imputaciones.Sum(imp => imp.MontoImputado);

            if (totalMetodos <= 0 || totalImputado <= 0)
            {
                return BadRequest("El monto del cobro debe ser mayor a cero.");
            }

            if (Math.Abs(totalMetodos - totalImputado) > 0.01m)
            {
                return BadRequest($"La suma de los métodos de pago (${totalMetodos:N2}) no coincide con el total de conceptos imputados (${totalImputado:N2}).");
            }

            try
            {
                var ok = await liquidacionRepositorio.RegistrarCobro(dto);
                if (!ok) return BadRequest("No se pudo registrar el cobro en la liquidación.");
                return Ok(new { mensaje = "Cobro registrado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar el cobro: " + ex.Message });
            }
        }

        // POST: api/liquidacion/anular-cobro
        [HttpPost("anular-cobro")]
        public async Task<IActionResult> AnularCobro([FromBody] AnularPagoLiquidacionDTO dto)
        {
            if (dto.PagoId <= 0 || string.IsNullOrWhiteSpace(dto.Motivo))
            {
                return BadRequest("Debe especificar el Id del cobro y el motivo de la anulación.");
            }

            if (string.IsNullOrWhiteSpace(dto.AnuladoPor))
                dto.AnuladoPor = ObtenerUsuarioActual();

            try
            {
                var ok = await liquidacionRepositorio.AnularCobro(dto);
                if (!ok) return BadRequest("No se pudo anular el cobro especificado.");
                return Ok(new { mensaje = "Cobro anulado y saldo revertido correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al anular el cobro: " + ex.Message });
            }
        }

        // POST: api/liquidacion/modificar-cobro
        [HttpPost("modificar-cobro")]
        public async Task<IActionResult> ModificarCobro([FromBody] ModificarPagoLiquidacionDTO dto)
        {
            if (dto.PagoId <= 0 || string.IsNullOrWhiteSpace(dto.Motivo) || dto.Metodos == null || dto.Metodos.Count == 0 || dto.Imputaciones == null || dto.Imputaciones.Count == 0)
            {
                return BadRequest("Debe especificar el cobro, el motivo de modificación, al menos un método de pago y al menos un concepto.");
            }

            if (string.IsNullOrWhiteSpace(dto.ModificadoPor))
                dto.ModificadoPor = ObtenerUsuarioActual();

            decimal totalMetodos = dto.Metodos.Sum(m => m.Monto);
            decimal totalImputado = dto.Imputaciones.Sum(imp => imp.MontoImputado);

            if (totalMetodos <= 0 || totalImputado <= 0)
            {
                return BadRequest("El monto del cobro modificado debe ser mayor a cero.");
            }

            if (Math.Abs(totalMetodos - totalImputado) > 0.01m)
            {
                return BadRequest($"La suma de los métodos de pago (${totalMetodos:N2}) no coincide con el total imputado (${totalImputado:N2}).");
            }

            try
            {
                var ok = await liquidacionRepositorio.ModificarCobro(dto);
                if (!ok) return BadRequest("No se pudo modificar el cobro especificado (puede que ya esté anulado o modificado).");
                return Ok(new { mensaje = "Cobro modificado y registrado en el historial correctamente." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al modificar el cobro: " + ex.Message });
            }
        }
    }
}
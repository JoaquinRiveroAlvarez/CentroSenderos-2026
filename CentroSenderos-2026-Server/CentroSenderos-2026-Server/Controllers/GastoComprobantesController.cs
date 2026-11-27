using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Server.Controllers;

[ApiController]
[Authorize(Policy = "EsSocio")]
[Route("api/gasto/{gastoId:int}/comprobantes")]
public sealed class GastoComprobantesController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GastoComprobanteDTO>>> Listar(int gastoId, CancellationToken cancellationToken)
    {
        if (!await ExisteGasto(gastoId, cancellationToken))
            return NotFound(new { mensaje = "No se encontró el gasto." });

        return await ComprobantesDelGasto(gastoId)
            .OrderByDescending(comprobante => comprobante.FechaCarga)
            .ThenByDescending(comprobante => comprobante.Id)
            .Select(comprobante => new GastoComprobanteDTO
            {
                Id = comprobante.Id,
                Nombre = comprobante.Nombre,
                TamanoBytes = comprobante.TamanoBytes,
                FechaCarga = comprobante.FechaCarga
            })
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{comprobanteId:int}")]
    public Task<IActionResult> Descargar(int gastoId, int comprobanteId, CancellationToken cancellationToken) =>
        ObtenerArchivo(gastoId, comprobanteId, descargar: true, cancellationToken);

    [HttpGet("{comprobanteId:int}/vista-previa")]
    public Task<IActionResult> VistaPrevia(int gastoId, int comprobanteId, CancellationToken cancellationToken) =>
        ObtenerArchivo(gastoId, comprobanteId, descargar: false, cancellationToken);

    private async Task<IActionResult> ObtenerArchivo(
        int gastoId, int comprobanteId, bool descargar, CancellationToken cancellationToken)
    {
        var comprobante = await ComprobantesDelGasto(gastoId)
            .Where(item => item.Id == comprobanteId)
            .Select(item => new { item.Contenido, item.TipoContenido, item.Nombre })
            .SingleOrDefaultAsync(cancellationToken);
        if (comprobante is null)
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "no-store";
        if (descargar)
            return File(comprobante.Contenido, comprobante.TipoContenido, comprobante.Nombre);

        if (comprobante.TipoContenido is not ("application/pdf" or "image/jpeg" or "image/png"))
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);

        Response.Headers["Content-Disposition"] = "inline";
        Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";
        return File(comprobante.Contenido, comprobante.TipoContenido, enableRangeProcessing: true);
    }

    private IQueryable<GastoComprobante> ComprobantesDelGasto(int gastoId) =>
        context.GastoComprobantes.AsNoTracking()
            .Where(item => item.GastoId == gastoId && (item.Gasto.EstadoRegistro == EnumEstadoRegistro.activo || item.Gasto.EstadoRegistro == EnumEstadoRegistro.anulado));

    private Task<bool> ExisteGasto(int gastoId, CancellationToken cancellationToken) =>
        context.Gastos.AnyAsync(
            gasto => gasto.Id == gastoId && (gasto.EstadoRegistro == EnumEstadoRegistro.activo || gasto.EstadoRegistro == EnumEstadoRegistro.anulado),
            cancellationToken);
}

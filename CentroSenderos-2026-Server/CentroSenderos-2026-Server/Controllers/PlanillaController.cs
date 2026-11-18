using System.Security.Claims;
using CentroSenderos_2026_BD;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using CentroSenderos_2026_Repositorio.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace CentroSenderos_2026_Server.Controllers
{
    [Authorize(Policy = "EsProfesional")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ApiController]
    [Route("api/planillas")]
    public class PlanillaController : ControllerBase
    {
        private bool Compartidas => User.IsInRole("Socio") || User.IsInRole("admin")
            || User.IsInRole("Calendario") || User.IsInRole("equipo");
        private string? Propietario => User.IsInRole("profesional")
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        private readonly ApplicationDbContext context;
        private readonly UserManager<MiUsuario> usuarios;
        private readonly IPlanillaRepositorio repositorio;
        private readonly ILogger<PlanillaController> logger;

        public PlanillaController(
            IPlanillaRepositorio repositorio,
            ILogger<PlanillaController> logger, ApplicationDbContext context, UserManager<MiUsuario> usuarios)
        {
            this.repositorio = repositorio;
            this.logger = logger;
            this.context = context;
            this.usuarios = usuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await repositorio.SelectListaPlanillas(Compartidas, Propietario);
            return Ok(lista);
        }

        [HttpPost]
        [RequestSizeLimit(22 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 22 * 1024 * 1024)]
        public async Task<IActionResult> Insertar(
            [FromForm] string nombre,
            [FromForm] int tipoPlanillaId,
            [FromForm] IFormFile archivo,
            [FromForm] string descripcion = "", [FromForm] bool? privada = null)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new
                {
                    mensaje = "Seleccioná un archivo PDF con contenido."
                });

            if (archivo.Length > 20 * 1024 * 1024)
                return BadRequest(new
                {
                    mensaje = "El archivo no puede superar los 20 MB."
                });

            try
            {
                using var ms = new MemoryStream();
                await archivo.CopyToAsync(ms);

                var esPrivada = privada ?? !Compartidas;
                var propietario = esPrivada ? Propietario : null;
                if ((esPrivada && propietario is null) || (!esPrivada && !Compartidas))
                    return Forbid();

                await using var transaccion = await context.Database.BeginTransactionAsync();
                if (propietario is not null)
                {
                    await context.Database.ExecuteSqlInterpolatedAsync(
                        $"""SELECT 1 FROM "AspNetUsers" WHERE "Id" = {propietario} FOR UPDATE""");
                    var usuario = await usuarios.FindByIdAsync(propietario);
                    if (usuario is null || !await usuarios.IsInRoleAsync(usuario, "profesional"))
                        return Forbid();
                }

                var id = await repositorio.InsertarPlanilla(
                    nombre,
                    descripcion,
                    tipoPlanillaId,
                    archivo.FileName,
                    ms.ToArray(), propietario);
                await transaccion.CommitAsync();

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al guardar una planilla.");

                return StatusCode(500, new
                {
                    mensaje = "No se pudo guardar la planilla. Intentá nuevamente."
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var detalle = await repositorio.ObtenerDetalle(id, Compartidas, Propietario);
            if (detalle == null) return NotFound(new { mensaje = "No se encontró la planilla." });
            return Ok(detalle);
        }

        [HttpPost("{id:int}/editar")]
        [RequestSizeLimit(22 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 22 * 1024 * 1024)]
        public async Task<IActionResult> Editar(int id, [FromForm] string nombre,
            [FromForm] int tipoPlanillaId, [FromForm] string descripcion = "", [FromForm] IFormFile? archivo = null)
        {
            if (archivo != null && (archivo.Length == 0 || archivo.Length > 20 * 1024 * 1024))
                return BadRequest(new { mensaje = "Seleccioná un PDF con contenido de hasta 20 MB." });
            try
            {
                byte[]? contenido = null;
                if (archivo != null)
                {
                    using var ms = new MemoryStream();
                    await archivo.CopyToAsync(ms);
                    contenido = ms.ToArray();
                }
                var actualizado = await repositorio.EditarPlanilla(id, nombre, descripcion,
                    tipoPlanillaId, archivo?.FileName, contenido, Compartidas, Propietario);
                if (!actualizado) return NotFound(new { mensaje = "No se encontró la planilla." });
                return Ok(id);
            }
            catch (ApplicationException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar una planilla.");
                return StatusCode(500, new { mensaje = "No se pudo actualizar la planilla. Intentá nuevamente." });
            }
        }

        [HttpGet("ver/{id:int}")]
        public async Task<IActionResult> Ver(int id)
        {
            var planilla = await repositorio.ObtenerArchivoCompleto(id, Compartidas, Propietario);

            if (planilla == null || planilla.Contenido.Length == 0)
                return NotFound(new
                {
                    mensaje = "No se encontró el archivo de la planilla."
                });

            var disposition = new ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(planilla.NombreGenerado);

            Response.Headers.ContentDisposition = disposition.ToString();
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(planilla.Contenido, "application/pdf");
        }

        [HttpGet("descargar/{id:int}")]
        public async Task<IActionResult> Descargar(int id)
        {
            var planilla = await repositorio.ObtenerArchivoCompleto(id, Compartidas, Propietario);

            if (planilla == null || planilla.Contenido.Length == 0)
                return NotFound(new
                {
                    mensaje = "No se encontró el archivo de la planilla."
                });

            return File(
                planilla.Contenido,
                "application/pdf",
                planilla.NombreGenerado);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await repositorio.DeletePlanilla(id, Compartidas, Propietario);

            if (!eliminado)
                return NotFound(new
                {
                    mensaje = "No se encontró la planilla."
                });

            return Ok(new { mensaje = "Planilla eliminada correctamente." });
        }
    }
}
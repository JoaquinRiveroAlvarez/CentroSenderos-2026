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
    [Authorize]
    [ApiController]
    [Route("api/planillas")]
    public class PlanillaController : ControllerBase
    {
        private readonly IPlanillaRepositorio repositorio;
        private readonly ILogger<PlanillaController> logger;

        public PlanillaController(
            IPlanillaRepositorio repositorio,
            ILogger<PlanillaController> logger)
        {
            this.repositorio = repositorio;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await repositorio.SelectListaPlanillas();
            return Ok(lista);
        }

        [HttpPost]
        [RequestSizeLimit(22 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 22 * 1024 * 1024)]
        public async Task<IActionResult> Insertar(
            [FromForm] string nombre,
            [FromForm] int tipoPlanillaId,
            [FromForm] IFormFile archivo,
            [FromForm] string descripcion = "")
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

                var id = await repositorio.InsertarPlanilla(
                    nombre,
                    descripcion,
                    tipoPlanillaId,
                    archivo.FileName,
                    ms.ToArray());

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

        [HttpGet("ver/{id:int}")]
        public async Task<IActionResult> Ver(int id)
        {
            var planilla = await repositorio.ObtenerArchivoCompleto(id);

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
            var planilla = await repositorio.ObtenerArchivoCompleto(id);

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
            var eliminado = await repositorio.DeletePlanilla(id);

            if (!eliminado)
                return NotFound(new
                {
                    mensaje = "No se encontró la planilla."
                });

            return Ok(new { mensaje = "Planilla eliminada correctamente." });
        }
    }
}
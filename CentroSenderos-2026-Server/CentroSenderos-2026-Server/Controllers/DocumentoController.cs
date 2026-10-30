using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Repositorio.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly IDocumentoRepositorio documentoRepo;

        public DocumentoController(IDocumentoRepositorio documentoRepo)
        {
            this.documentoRepo = documentoRepo;
        }

        [HttpPost("insertar")]
        [RequestSizeLimit(35 * 1024 * 1024)] // 35 MB de límite máximo
        [RequestFormLimits(MultipartBodyLengthLimit = 35 * 1024 * 1024)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Insertar([FromForm] int pacienteId, [FromForm] int tipoDocumentoId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se proporcionó ningún archivo válido.");
            }

            try
            {
                using var ms = new MemoryStream();
                await archivo.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
                    ? "application/octet-stream"
                    : archivo.ContentType;

                var id = await documentoRepo.InsertarDocumentoConArchivo(
                    pacienteId,
                    tipoDocumentoId,
                    archivo.FileName,
                    contentType,
                    bytes
                );

                var documento = await documentoRepo.SelectPorId(id);
                return Ok(documento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar el archivo: {ex.Message}");
            }
        }

        [HttpGet("ver/{id}")]
        public async Task<IActionResult> Ver(int id)
        {
            var doc = await documentoRepo.ObtenerArchivoCompleto(id);
            if (doc == null || doc.Contenido == null || doc.Contenido.Length == 0)
            {
                return NotFound("El archivo no fue encontrado o no tiene contenido.");
            }

            var contentType = string.IsNullOrWhiteSpace(doc.ContentType)
                ? "application/octet-stream"
                : doc.ContentType;

            // ContentDispositionHeaderValue con SetHttpFileName para compatibilidad con tildes (RFC 6266 / 5987)
            var cd = new ContentDispositionHeaderValue("inline");
            cd.SetHttpFileName(doc.NombreGenerado);
            Response.Headers.ContentDisposition = cd.ToString();

            return File(doc.Contenido, contentType);
        }

        [HttpGet("descargar/{id}")]
        public async Task<IActionResult> Descargar(int id)
        {
            var doc = await documentoRepo.ObtenerArchivoCompleto(id);
            if (doc == null || doc.Contenido == null || doc.Contenido.Length == 0)
            {
                return NotFound("El archivo no fue encontrado o no tiene contenido.");
            }

            var contentType = string.IsNullOrWhiteSpace(doc.ContentType)
                ? "application/octet-stream"
                : doc.ContentType;

            // Al pasar el tercer argumento (fileDownloadName), File() fuerza la descarga como 'attachment'
            return File(doc.Contenido, contentType, doc.NombreGenerado);
        }

        [HttpGet("tipos")]
        public async Task<IActionResult> GetTipos()
        {
            var tipos = await documentoRepo.SelectTipos();
            return Ok(tipos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var ok = await documentoRepo.DeleteDocumento(id);
            return ok ? Ok() : NotFound();
        }
    }
}
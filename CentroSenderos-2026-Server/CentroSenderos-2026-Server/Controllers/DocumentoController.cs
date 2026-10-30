using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Repositorio.Repositorios;
using Microsoft.AspNetCore.Mvc;

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
        [RequestSizeLimit(30 * 1024 * 1024)] // Límite de 30 MB
        public async Task<IActionResult> Insertar([FromForm] int pacienteId, [FromForm] int tipoDocumentoId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se proporcionó ningún archivo.");

            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var id = await documentoRepo.InsertarDocumentoConArchivo(
                pacienteId,
                tipoDocumentoId,
                archivo.FileName,
                archivo.ContentType,
                bytes
            );

            var documento = await documentoRepo.SelectPorId(id);
            return Ok(documento);
        }

        [HttpGet("ver/{id}")]
        public async Task<IActionResult> Ver(int id)
        {
            var doc = await documentoRepo.ObtenerArchivoCompleto(id);
            if (doc == null || doc.Contenido.Length == 0)
                return NotFound("Archivo no encontrado.");

            // inline permite visualizar en el navegador (iframe, img, pdf)
            Response.Headers.Append("Content-Disposition", $"inline; filename=\"{doc.NombreGenerado}\"");
            return File(doc.Contenido, doc.ContentType);
        }

        [HttpGet("descargar/{id}")]
        public async Task<IActionResult> Descargar(int id)
        {
            var doc = await documentoRepo.ObtenerArchivoCompleto(id);
            if (doc == null || doc.Contenido.Length == 0)
                return NotFound("Archivo no encontrado.");

            return File(doc.Contenido, doc.ContentType, doc.NombreOriginal.Length > 0 ? doc.NombreOriginal : doc.NombreGenerado);
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
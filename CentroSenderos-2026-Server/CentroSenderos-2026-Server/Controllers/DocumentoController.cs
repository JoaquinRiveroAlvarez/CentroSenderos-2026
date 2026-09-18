using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Servicio;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly IDocumentoRepositorio documentoRepo;
        private readonly SupabaseStorageService supabase;

        public DocumentoController(IDocumentoRepositorio documentoRepo, SupabaseStorageService supabase)
        {
            this.documentoRepo = documentoRepo;
            this.supabase = supabase;
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> Insertar([FromForm] DocumentoDTO dto, IFormFile archivo)
        {
            using var stream = archivo.OpenReadStream();
            var url = await supabase.SubirDocumentoAsync(stream, archivo.FileName, archivo.ContentType);

            dto.UrlArchivo = url;
            var id = await documentoRepo.InsertarDocumento(dto);

            var documento = await documentoRepo.SelectPorId(id);
            return Ok(documento);
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


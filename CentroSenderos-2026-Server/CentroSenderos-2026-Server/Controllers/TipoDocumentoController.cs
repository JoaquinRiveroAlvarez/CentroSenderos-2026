using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipodocumento")]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly ITipoDocumentoRepositorio repositorio;

        public TipoDocumentoController(ITipoDocumentoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoDocumentoDTO>> GetById(int id)
        {
            var entidad = await repositorio.SelectPorId(id);
            if (entidad == null) return NotFound($"No existe tipo de documento con id {id}.");
            return Ok(entidad);
        }

        [HttpGet("ListaTipoDocumentos")]
        public async Task<IActionResult> GetListaTipoDocumento()
        {
            try
            {
                var lista = await repositorio.SelectListaTipoDocumento();
                if (lista == null || !lista.Any())
                    return NotFound("No hay tipos de documentos registrados.");

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener tipos de documentos: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> PostDocumento([FromBody] TipoDocumentoDTO dto)
        {
            try
            {
                int id = await repositorio.InsertarTipoDocumento(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, TipoDocumentoDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarTipoDocumento(id, dto);
                if (!resultado)
                {
                    return NotFound($"No existe el tipo de documento con el id: {id}.");
                }
                return Ok(new { mensaje = "Actualizado correctamente", id, datos = dto });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado = await repositorio.DeleteTipoDocumento(id);
            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }
            return Ok(new { mensaje = "Eliminado correctamente", id });
        }
    }
}

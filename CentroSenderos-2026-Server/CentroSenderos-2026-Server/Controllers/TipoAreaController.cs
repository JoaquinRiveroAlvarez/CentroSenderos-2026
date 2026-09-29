using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipoarea")]
    public class TipoAreaController : ControllerBase
    {
        private readonly ITipoAreaRepositorio repositorio;

        public TipoAreaController(ITipoAreaRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaTipoArea")]
        public async Task<ActionResult<List<TipoListadoDTO>>> GetLista()
        {
            var lista = await repositorio.SelectListaTipoArea();

            // Una lista vacía es un resultado válido para el formulario.
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoDTO>> GetPorId(int id)
        {
            var area = await repositorio.SelectPorId(id);

            if (area is null)
                return NotFound();

            return Ok(area);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] TipoDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarTipoArea(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(
            int id,
            [FromBody] TipoDTO dto)
        {
            try
            {
                var actualizado =
                    await repositorio.ActualizarTipoArea(id, dto);

                if (!actualizado)
                    return NotFound();

                return Ok(new RespuestaDTO
                {
                    mensaje = "Tipo de área actualizado correctamente."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var eliminado = await repositorio.DeleteTipoArea(id);

                if (!eliminado)
                    return NotFound();

                return Ok(new RespuestaDTO
                {
                    mensaje = "Tipo de área eliminado correctamente."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
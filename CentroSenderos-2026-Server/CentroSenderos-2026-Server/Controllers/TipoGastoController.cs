using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipogasto")]
    public class TipoGastoController : ControllerBase
    {
        private readonly ITipoGastoRepositorio repositorio;

        public TipoGastoController(ITipoGastoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaTipoGasto")]
        public async Task<ActionResult<List<TipoListadoDTO>>> GetLista()
        {
            var lista = await repositorio.SelectListaTipoGasto();
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoDTO>> GetPorId(int id)
        {
            var tipoGasto = await repositorio.SelectPorId(id);

            if (tipoGasto is null)
                return NotFound();

            return Ok(tipoGasto);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] TipoDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarTipoGasto(dto);
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
                    await repositorio.ActualizarTipoGasto(id, dto);

                if (!actualizado)
                    return NotFound();

                return Ok(new RespuestaDTO
                {
                    mensaje = "Tipo de gasto actualizado correctamente."
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
                var eliminado = await repositorio.DeleteTipoGasto(id);

                if (!eliminado)
                    return NotFound();

                return Ok(new RespuestaDTO
                {
                    mensaje = "Tipo de gasto eliminado correctamente."
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
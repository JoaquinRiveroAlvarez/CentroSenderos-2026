using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
        [ApiController]
        [Route("api/tipoplanillas")]
        public class TipoPlanillaController : ControllerBase
        {
            private readonly ITipoPlanillaRepositorio repositorio;

            public TipoPlanillaController(ITipoPlanillaRepositorio repositorio)
            {
                this.repositorio = repositorio;
            }

            [HttpGet("{id:int}")]
            public async Task<ActionResult<TipoDTO>> GetById(int id)
            {
                var entidad = await repositorio.SelectPorId(id);
                if (entidad == null) return NotFound($"No existe diagnóstico con id {id}.");
                return Ok(entidad);
            }

            [HttpGet("ListaTipoPlanillas")]
            public async Task<IActionResult> GetListaTipoPlanilla()
            {
                try
                {
                    var lista = await repositorio.SelectListaTipoPlanilla();
                    if (lista == null || !lista.Any())
                        return NotFound("No hay diagnósticos registrados.");

                    return Ok(lista);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error al obtener diagnósticos: {ex.Message}");
                }
            }


            [HttpPost]
            public async Task<IActionResult> PostPlanilla([FromBody] TipoDTO dto)
            {
                try
                {
                    int id = await repositorio.InsertarTipoPlanilla(dto);
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
            public async Task<ActionResult> Put(int id, TipoDTO dto)
            {
                try
                {
                    var resultado = await repositorio.ActualizarTipoPlanilla(id, dto);
                    if (!resultado)
                    {
                        return NotFound($"No existe la planilla con el id: {id}.");
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
                var resultado = await repositorio.DeleteTipoPlanilla(id);
                if (!resultado)
                {
                    return BadRequest("Datos no válidos");
                }
                return Ok(new { mensaje = "Eliminado correctamente", id });
            }
        }
}

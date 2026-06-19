using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/tipoturno")]
    public class TipoTurnoController : ControllerBase
    {
        private readonly ITipoTurnoRepositorio repositorio;

        public TipoTurnoController(ITipoTurnoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaTipoTurno")]
        public async Task<IActionResult> GetListaTipoTurno()
        {
            var lista = await repositorio.SelectListaTipoTurno();
            if (lista == null || !lista.Any())
                return NotFound("No hay tipos de turno registrados.");

            return Ok(lista);
        }

        [HttpGet("Tipo/{cod}")]
        public async Task<ActionResult<TipoTurnoListadoDTO>> SelectByTipo(string cod)
        {
            var tipoPrestacion = await repositorio.SelectByTipoTurno(cod);
            if (tipoPrestacion is null)
            {
                return NotFound($"No existe el registro con el tipo: {cod}.");
            }

            return Ok(tipoPrestacion);
        }

        [HttpPost]
        public async Task<IActionResult> PostObraSocial([FromBody] TipoTurnoDTO dto)
        {
            try
            {
                int id = await repositorio.InsertarTipoTurno(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                // Esto devuelve el mensaje controlado al cliente
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, TipoTurnoDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarTipoTurno(id, dto);
                if (!resultado)
                {
                    return NotFound($"No existe el tipo de turno con el id: {id}.");
                }

                return Ok($"El registro con el id: {id} fue actualizado correctamente.");
            }
            catch (ApplicationException ex)
            {
                // Esto devuelve el mensaje controlado al cliente
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado = await repositorio.DeleteTipoTurno(id);
            if (!resultado)
            {
                return BadRequest("Datos no validos");
            }
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }

        //[HttpGet("ListaProfesionales")]
        //public async Task<ActionResult<List<ProfesionalListadoDTO>>> GetListaProfesional()
        //{
        //    var lista = await repositorio.SelectListaProfesional();
        //    if (lista == null)
        //    {
        //        return NotFound("No se encontro la lista, VERIFICAR.");
        //    }
        //    if (lista.Count == 0)
        //    {
        //        return Ok("No existen items en la lista en este momento");
        //    }
        //    return Ok(lista);
        //}
    }
}

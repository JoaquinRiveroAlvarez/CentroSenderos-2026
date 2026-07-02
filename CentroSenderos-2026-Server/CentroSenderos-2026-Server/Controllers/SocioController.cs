using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/socio")]
    public class SocioController : ControllerBase
    {
        private readonly ISocioRepositorio repositorio;

        public SocioController(ISocioRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaSocio")]
        public async Task<ActionResult<List<SocioListadoDTO>>> GetListaSocio()
        {
            var lista = await repositorio.SelectListaSocios();
            if (lista == null)
            {
                return NotFound("No se encontró la lista de socios, VERIFICAR.");
            }
            if (lista.Count == 0)
            {
                return Ok("No existen socios en la lista en este momento");
            }
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SocioListadoDTO>> GetSocioPorId(int id)
        {
            var socio = await repositorio.SelectPorId(id);
            if (socio == null)
            {
                return NotFound(new { message = $"No se encontró el socio con id {id}" });
            }
            return Ok(socio);
        }

        [HttpGet("profesional/{profesionalId}")]
        public async Task<ActionResult<SocioListadoDTO>> GetSocioPorProfesional(int profesionalId)
        {
            var socio = await repositorio.SelectByProfesionalId(profesionalId);
            if (socio == null)
            {
                return NotFound(new { message = $"No se encontró socio para el profesional con id {profesionalId}" });
            }
            return Ok(socio);
        }

        [HttpPost("insertar")]
        public async Task<ActionResult> InsertarSocio([FromBody] SocioDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarSocio(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor",
                    detalle = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, SocioDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarSocio(id, dto);
                if (!resultado)
                {
                    return NotFound(new { mensaje = $"No existe el socio con el id: {id}." });
                }

                return Ok(new { mensaje = $"El socio con el id: {id} fue actualizado correctamente." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor",
                    detalle = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var resultado = await repositorio.DeleteSocio(id);
            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }
            return Ok($"El socio con el id: {id} fue eliminado correctamente.");
        }
    }
}


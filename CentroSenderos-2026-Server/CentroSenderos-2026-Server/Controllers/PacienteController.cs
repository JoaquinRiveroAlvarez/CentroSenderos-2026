using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/paciente")]
    public class PacienteController : ControllerBase
    {
        private readonly IPacienteRepositorio repositorio;

        public PacienteController(IPacienteRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("ListaPaciente")]
        public async Task<ActionResult<List<PacienteResumenDTO>>> GetListaPaciente()
        {
            var lista = await repositorio.SelectListaPaciente();
            if (lista == null)
            {
                return NotFound("No se encontró la lista, VERIFICAR.");
            }
            if (lista.Count == 0)
            {
                return Ok("No existen pacientes en la lista en este momento");
            }
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDTO>> GetPacientePorId(int id)
        {
            var paciente = await repositorio.SelectPorId(id);
            if (paciente == null)
            {
                return NotFound(new { message = $"No se encontró el paciente con id {id}" });
            }
            return Ok(paciente);
        }

        [HttpPost("insertar")]
        public async Task<ActionResult> InsertarPaciente([FromBody] PacienteCrearDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarPaciente(dto);
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                // Errores controlados (ej: DNI duplicado, obra social inexistente)
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados: mostramos detalle e inner exception
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor",
                    detalle = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, PacienteDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarPaciente(id, dto);
                if (!resultado)
                {
                    return NotFound(new { mensaje = $"No existe el paciente con el id: {id}." });
                }

                return Ok(new { mensaje = $"El paciente con el id: {id} fue actualizado correctamente." });
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
            var resultado = await repositorio.Delete(id);
            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }
    }
}

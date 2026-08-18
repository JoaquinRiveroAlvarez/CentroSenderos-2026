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
                return NotFound(
                    new { mensaje = "No se pudo obtener la lista de pacientes." }
                );
            }
            if (lista.Count == 0)
            {
                return Ok("No existen pacientes en la lista en este momento");
            }
            return Ok(lista);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<PacienteDTO>> GetPacientePorId(int id)
        {
            var paciente = await repositorio.SelectPorId(id);

            if (paciente == null)
            {
                return NotFound(
                    new { mensaje = $"No se encontró el paciente con id {id}." }
                );
            }

            return Ok(paciente);
        }


        [HttpPost("insertar")]
        public async Task<ActionResult<int>> InsertarPaciente(
            [FromBody] PacienteCrearDTO dto)
        {
            try
            {
                var id = await repositorio.InsertarPaciente(dto);

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new { mensaje = ex.Message }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al registrar el paciente."
                    }
                );
            }
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(
            int id,
            [FromBody] PacienteDTO dto)
        {
            try
            {
                var resultado =
                    await repositorio.ActualizarPaciente(id, dto);

                if (!resultado)
                {
                    return NotFound(
                        new { mensaje = $"No se encontró el paciente con id {id}." }
                    );
                }

                return Ok(
                    new
                    {
                        mensaje = "El paciente fue actualizado correctamente."
                    }
                );
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new { mensaje = ex.Message }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al actualizar el paciente."
                    }
                );
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var resultado =
                    await repositorio.DeletePaciente(id);

                if (!resultado)
                {
                    return NotFound(
                        new { mensaje = $"No se encontró el paciente con id {id}." }
                    );
                }

                return Ok(
                    new
                    {
                        mensaje = "El paciente fue eliminado correctamente."
                    }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje = "Ocurrió un error inesperado al eliminar el paciente."
                    }
                );
            }
        }
    }
}
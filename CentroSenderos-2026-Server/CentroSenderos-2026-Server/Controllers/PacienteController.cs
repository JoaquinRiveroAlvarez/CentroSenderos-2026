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

        //[HttpGet("{id}")]
        //public async Task<ActionResult<PacienteResumenDTO>> GetPacientePorId(int id)
        //{
        //    var paciente = await repositorio.SelectPorId(id);
        //    if (paciente == null)
        //    {
        //        return NotFound(new { message = $"No se encontró el paciente con id {id}" });
        //    }
        //    return Ok(paciente);
        //}

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePaciente(int id, [FromBody] PacienteEditarDTO dto)
        {
            var result = await repositorio.UpdatePacienteActualizar(id, dto);

            if (!result)
                return NotFound(new { message = $"No se encontró el paciente con id {id}" });

            return Ok(new { message = $"Paciente {id} actualizado correctamente" });
        }

        [HttpPost("crear")]
        public async Task<ActionResult> CrearPaciente([FromBody] PacienteCrearDTO dto)
        {
            try
            {
                var id = await repositorio.CrearPaciente(dto);
                return Ok(new { message = $"Paciente creado correctamente con Id {id}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear el paciente", detail = ex.Message });
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

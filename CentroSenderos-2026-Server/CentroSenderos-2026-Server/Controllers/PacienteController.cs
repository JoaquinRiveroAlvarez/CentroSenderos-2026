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
        public async Task<ActionResult<PacienteDetalleDTO>> GetPacientePorId(int id)
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
                return Ok(new { message = $"Paciente creado correctamente con Id {id}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear el paciente", detail = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] PacienteEditarDTO dto)
        {
            var entidad = new Paciente
            {
                Id = id,
                Nombre = dto.Nombre,
                DNI = dto.DNI,
                Genero = dto.Genero,
                FechaNacimiento = dto.FechaNacimiento,
                TipoObraSocialId = dto.TipoObraSocialId,
                NumeroAfiliado = dto.NumeroAfiliado,
                TipoDiagnosticoId = dto.TipoDiagnosticoId,
                ProfesionalId = dto.ProfesionalId,
                Telefono = dto.Telefono,
                Domicilio = dto.Domicilio,
                CorreoElectronico = dto.CorreoElectronico,
                TipoDocumentoId = dto.TipoDocumentoId,
                EstadoRegistro = dto.EstadoRegistro
            };

            var resultado = await repositorio.Update(id, entidad);

            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }

            return Ok($"El registro con el id: {id} fue actualizado correctamente.");
        }



        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeletePaciente(int id)
        {
            var resultado = await repositorio.DeletePaciente(id);
            if (!resultado)
            {
                return BadRequest("Datos no válidos");
            }
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }
    }
}

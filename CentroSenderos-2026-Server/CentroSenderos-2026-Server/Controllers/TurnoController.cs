using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/turno")]
    public class TurnoController : ControllerBase
    {
        private readonly ITurnoRepositorio repositorio;

        public TurnoController(ITurnoRepositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TurnoDTO>> GetById(int id)
        {
            var entidad = await repositorio.SelectPorId(id);
            if (entidad == null) return NotFound($"No existe turno con id {id}.");
            return Ok(entidad);
        }

        [HttpGet("ListaTurnos")]
        public async Task<IActionResult> GetListaTurnos()
        {
            var lista = await repositorio.SelectListaTurnos();
            if (lista == null || !lista.Any())
                return NotFound("No hay turnos disponibles.");
            return Ok(lista);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] TurnoDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarTurno(id, dto);
                if (!resultado)
                {
                    return NotFound(
                        new RespuestaDTO
                        {
                            mensaje =
                                $"No se encontró el turno con id {id} " +
                                "al intentar actualizarlo."
                        }
                    );
                }
                return Ok($"Turno {id} actualizado correctamente.");

            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TurnoDTO dto)
        {
                try
                {
                    int id = await repositorio.InsertarTurno(dto);
                    return Ok(id);
                }
                catch (ApplicationException ex)
                {
                    return BadRequest(new { mensaje = ex.Message });
                }
        }

        [HttpGet("Disponibles")]
        public async Task<ActionResult<List<string>>> GetDisponibles(DateOnly fecha,int tipoTurnoId,int consultorioId,[FromQuery] List<int>? profesionalIds = null,[FromQuery] List<int>? pacienteIds = null)
        {
            try
            {
                var horarios = await repositorio
                    .HorariosDisponibles(
                        fecha,
                        tipoTurnoId,
                        consultorioId,
                        profesionalIds,
                        pacienteIds
                    );

                var lista = horarios
                    .Select(hora =>
                        hora.ToString("HH:mm")
                    )
                    .ToList();

                return Ok(lista);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new
                    {
                        mensaje = ex.Message
                    }
                );
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await repositorio.DeleteTurno(id);
            if (!resultado) return NotFound($"No existe turno con id {id}.");
            return Ok($"Turno {id} eliminado correctamente.");
        }
    }
}


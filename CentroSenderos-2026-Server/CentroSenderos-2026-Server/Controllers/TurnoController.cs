using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/turno")]
    public class TurnoController : ControllerBase
    {
        private readonly ITurnoRepositorio repositorio;
        private readonly ILogger<TurnoController> logger;

        public TurnoController(
            ITurnoRepositorio repositorio,
            ILogger<TurnoController> logger
        )
        {
            this.repositorio = repositorio;
            this.logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TurnoDTO>> GetById(int id)
        {
            try
            {
                var entidad =
                    await repositorio.SelectPorId(id);

                if (entidad is null)
                {
                    return NotFound(
                        new RespuestaDTO
                        {
                            mensaje =
                                "No encontramos el turno solicitado."
                        }
                    );
                }

                return Ok(entidad);
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "cargar el turno",
                    "No pudimos cargar el turno en este momento. " +
                    "Intentá nuevamente."
                );
            }
        }

        [HttpGet("ListaTurnos")]
        public async Task<IActionResult> GetListaTurnos()
        {
            try
            {
                var lista =
                    await repositorio.SelectListaTurnos();

                if (lista is null || !lista.Any())
                {
                    return NotFound(
                        "No hay turnos disponibles."
                    );
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "cargar el listado de turnos",
                    "No pudimos cargar los turnos en este momento. " +
                    "Intentá nuevamente."
                );
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] TurnoDTO dto
        )
        {
            try
            {
                var resultado =
                    await repositorio.ActualizarTurno(
                        id,
                        dto
                    );

                if (!resultado)
                {
                    return NotFound(
                        new RespuestaDTO
                        {
                            mensaje =
                                "No encontramos el turno que querés actualizar."
                        }
                    );
                }

                var mensaje = dto.ModificarTodaLaSerie
                    ? "Los turnos futuros de la serie se actualizaron correctamente."
                    : "El turno se actualizó correctamente.";

                return Ok(
                    new RespuestaDTO
                    {
                        mensaje = mensaje
                    }
                );
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new RespuestaDTO
                    {
                        mensaje = ex.Message
                    }
                );
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "actualizar el turno",
                    "No pudimos guardar los cambios del turno. " +
                    "Intentá nuevamente."
                );
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] TurnoDTO dto
        )
        {
            try
            {
                var id =
                    await repositorio.InsertarTurno(dto);

                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(
                    new RespuestaDTO
                    {
                        mensaje = ex.Message
                    }
                );
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "crear el turno",
                    "No pudimos crear el turno en este momento. " +
                    "Intentá nuevamente."
                );
            }
        }

        [HttpGet("Disponibles")]
        public async Task<ActionResult<List<string>>> GetDisponibles(
            DateOnly fecha,
            int tipoTurnoId,
            int consultorioId,
            [FromQuery] List<int>? profesionalIds = null,
            [FromQuery] List<int>? pacienteIds = null,
            int? turnoIdExcluir = null
        )
        {
            try
            {
                var horarios =
                    await repositorio.HorariosDisponibles(
                        fecha,
                        tipoTurnoId,
                        consultorioId,
                        profesionalIds,
                        pacienteIds,
                        turnoIdExcluir
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
                    new RespuestaDTO
                    {
                        mensaje = ex.Message
                    }
                );
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "consultar los horarios disponibles",
                    "No pudimos consultar los horarios disponibles. " +
                    "Intentá nuevamente."
                );
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado =
                    await repositorio.DeleteTurno(id);

                if (!resultado)
                {
                    return NotFound(
                        new RespuestaDTO
                        {
                            mensaje =
                                "No encontramos el turno que querés eliminar."
                        }
                    );
                }

                return Ok(
                    new RespuestaDTO
                    {
                        mensaje =
                            "El turno se eliminó correctamente."
                    }
                );
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "eliminar el turno",
                    "No pudimos eliminar el turno en este momento. " +
                    "Intentá nuevamente."
                );
            }
        }

        private ObjectResult ErrorInterno(
            Exception exception,
            string operacion,
            string mensaje
        )
        {
            logger.LogError(
                exception,
                "Error inesperado al {Operacion}.",
                operacion
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new RespuestaDTO
                {
                    mensaje = mensaje
                }
            );
        }
    }
}

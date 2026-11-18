using CentroSenderos_2026_BD;
using Microsoft.EntityFrameworkCore;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace CentroSenderos_2026_Server.Controllers
{
    [Authorize(Policy = "EsProfesional")]
    [ApiController]
    [Route("api/turno")]
    public class TurnoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ITurnoRepositorio repositorio;
        private readonly ILogger<TurnoController> logger;

        public TurnoController(
            ITurnoRepositorio repositorio,
            ILogger<TurnoController> logger,
            ApplicationDbContext context
        )
        {
            this.repositorio = repositorio;
            this.logger = logger;
            this.context = context;
        }

        private IQueryable<CentroSenderos_2026_BD.Datos.Entity.Turno> TurnosPermitidos()
        {
            if (AlcanceProfesional.TieneAccesoCompleto(User))
                return context.Turnos;

            var ids = AlcanceProfesional.Profesionales(context, User);
            return context.Turnos.Where(t => t.TurnoProfesionales.Any()
                && t.TurnoProfesionales.All(p => ids.Contains(p.ProfesionalId)));
        }

        private async Task<bool> PuedeGestionarTurno(int id, bool serie = false)
        {
            if (AlcanceProfesional.TieneAccesoCompleto(User))
                return true;

            var permitidos = TurnosPermitidos();
            var turno = await permitidos.Where(t => t.Id == id)
                .Select(t => new { t.SerieTurnoId }).FirstOrDefaultAsync();
            return turno is not null && (!serie || turno.SerieTurnoId is null
                || !await context.Turnos.AnyAsync(t => t.SerieTurnoId == turno.SerieTurnoId
                    && !permitidos.Any(p => p.Id == t.Id)));
        }

        private async Task<bool> PuedeGestionarProfesionales(IEnumerable<int> ids)
        {
            if (AlcanceProfesional.TieneAccesoCompleto(User))
                return true;
            var solicitados = ids.Where(id => id > 0).Distinct().ToArray();
            var permitidos = AlcanceProfesional.Profesionales(context, User);
            return solicitados.Length == await permitidos.Distinct().CountAsync(id => solicitados.Contains(id));
        }

        private Task<bool> PuedeGestionarProfesionales(TurnoDTO dto) =>
            PuedeGestionarProfesionales(dto.ProfesionalIds
                .Concat(dto.ProfesionalPrestaciones.Select(p => p.ProfesionalId)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TurnoDTO>> GetById(int id)
        {
            try
            {
                if (!await PuedeGestionarTurno(id)) return NotFound();
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

                if (lista is not null && !AlcanceProfesional.TieneAccesoCompleto(User))
                {
                    var permitidos = (await TurnosPermitidos().Select(t => t.Id).ToListAsync()).ToHashSet();
                    lista = lista.Where(t => permitidos.Contains(t.Id)).ToList();
                }

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
        public async Task<IActionResult> Put(int id,[FromBody] TurnoDTO dto)
        {
            try
            {
                if (!await PuedeGestionarTurno(id, dto.ModificarTodaLaSerie)
                    || !await PuedeGestionarProfesionales(dto)) return Forbid();
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
        public async Task<IActionResult> Post([FromBody] TurnoDTO dto)
        {
            try
            {
                if (!await PuedeGestionarProfesionales(dto)) return Forbid();
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
            int? turnoIdExcluir = null,
            [FromQuery] int intervaloMinutos = 5
        )
        {
            try
            {
                if (!await PuedeGestionarProfesionales(profesionalIds ?? [])
                    || (turnoIdExcluir.HasValue && !await PuedeGestionarTurno(turnoIdExcluir.Value)))
                    return Forbid();
                var horarios =
                    await repositorio.HorariosDisponibles(
                        fecha,
                        tipoTurnoId,
                        consultorioId,
                        profesionalIds,
                        pacienteIds,
                        turnoIdExcluir,
                        intervaloMinutos
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

        [HttpGet("Horarios")]
        public async Task<ActionResult<List<HorarioTurnoDTO>>> GetHorarios(
            DateOnly fecha,
            int tipoTurnoId,
            int consultorioId,
            [FromQuery] List<int>? profesionalIds = null,
            [FromQuery] List<int>? pacienteIds = null,
            int? turnoIdExcluir = null,
            [FromQuery] int intervaloMinutos = 5,
            [FromQuery] TimeOnly? hora = null
        )
        {
            try
            {
                if (!await PuedeGestionarProfesionales(profesionalIds ?? [])
                    || (turnoIdExcluir.HasValue && !await PuedeGestionarTurno(turnoIdExcluir.Value)))
                    return Forbid();
                var horarios =
                    await repositorio.HorariosConOcupacion(
                        fecha,
                        tipoTurnoId,
                        consultorioId,
                        profesionalIds,
                        pacienteIds,
                        turnoIdExcluir,
                        intervaloMinutos,
                        hora
                    );

                if (!AlcanceProfesional.TieneAccesoCompleto(User))
                {
                    var ids = horarios.SelectMany(h => h.Conflictos).Select(c => c.TurnoId).Distinct().ToArray();
                    var permitidos = (await TurnosPermitidos().Where(t => ids.Contains(t.Id))
                        .Select(t => t.Id).ToListAsync()).ToHashSet();
                    foreach (var conflicto in horarios.SelectMany(h => h.Conflictos)
                        .Where(c => !permitidos.Contains(c.TurnoId)))
                    {
                        conflicto.TurnoId = 0;
                        conflicto.Profesionales.Clear();
                        conflicto.Pacientes.Clear();
                    }
                }

                return Ok(horarios);
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

        [HttpGet("VerificarDisponibilidad")]
        public async Task<ActionResult<bool>> VerificarDisponibilidad(
            DateOnly fecha,
            TimeOnly hora,
            int tipoTurnoId,
            int consultorioId,
            [FromQuery] List<int>? profesionalIds = null,
            [FromQuery] List<int>? pacienteIds = null,
            int? turnoIdExcluir = null
        )
        {
            try
            {
                if (!await PuedeGestionarProfesionales(profesionalIds ?? [])
                    || (turnoIdExcluir.HasValue && !await PuedeGestionarTurno(turnoIdExcluir.Value)))
                    return Forbid();
                var disponible = await repositorio.EsHorarioDisponible(
                    fecha,
                    hora,
                    tipoTurnoId,
                    consultorioId,
                    profesionalIds,
                    pacienteIds,
                    turnoIdExcluir
                );

                return Ok(disponible);
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
                    "verificar disponibilidad del horario",
                    "No pudimos verificar el horario. Intentá nuevamente."
                );
            }
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await PuedeGestionarTurno(id)) return Forbid();
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

        private ObjectResult ErrorInterno(Exception exception,string operacion,string mensaje)
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

        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id,[FromBody]CentroSenderos_2026_Shared.Enum.EnumEstadoTurno estado)
        {
            try
            {
                if (!await PuedeGestionarTurno(id)) return Forbid();
                var actualizado = await repositorio.CambiarEstadoTurno(
                    id,
                    estado
                );

                if (!actualizado)
                {
                    return NotFound(new RespuestaDTO
                    {
                        mensaje = "No encontramos el turno solicitado."
                    });
                }

                return Ok(new RespuestaDTO
                {
                    mensaje = "El estado del turno se actualizó correctamente."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "cambiar el estado del turno",
                    "No pudimos actualizar el estado. Intentá nuevamente."
                );
            }
        }

        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> CancelarTurnos(int id,[FromQuery] bool cancelarSiguientes = false)
        {
            try
            {
                if (!await PuedeGestionarTurno(id, cancelarSiguientes)) return Forbid();
                var cantidadCancelada = await repositorio.CancelarTurnos(
                    id,
                    cancelarSiguientes
                );

                var mensaje = cantidadCancelada == 1
                    ? "Se canceló 1 turno correctamente."
                    : $"Se cancelaron {cantidadCancelada} turnos correctamente.";

                return Ok(new RespuestaDTO
                {
                    mensaje = mensaje
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "cancelar turnos",
                    "No pudimos cancelar los turnos. Intentá nuevamente."
                );
            }
        }

        [HttpPut("{turnoId:int}/pacientes/{pacienteId:int}/asistencia")]
        public async Task<IActionResult> CambiarAsistenciaPaciente(int turnoId,int pacienteId,[FromBody] TurnoPacienteAsistenciaDTO dto)
        {
            try
            {
                if (!await PuedeGestionarTurno(turnoId)) return Forbid();
                var actualizado = await repositorio.CambiarAsistenciaPaciente(
                    turnoId,
                    pacienteId,
                    dto.Asistencia
                );

                if (!actualizado)
                {
                    return NotFound(new RespuestaDTO
                    {
                        mensaje =
                            "No encontramos al paciente asignado a ese turno."
                    });
                }

                return Ok(new RespuestaDTO
                {
                    mensaje = "La asistencia se actualizó correctamente."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new RespuestaDTO
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return ErrorInterno(
                    ex,
                    "actualizar la asistencia del paciente",
                    "No pudimos guardar la asistencia. Intentá nuevamente."
                );
            }
        }
    }
}

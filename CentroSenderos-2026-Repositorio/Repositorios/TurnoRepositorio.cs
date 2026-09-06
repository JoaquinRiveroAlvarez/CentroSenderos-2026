using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Recurrencias;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TurnoRepositorio :
        Repositorio<Turno>,
        ITurnoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TurnoRepositorio(
            ApplicationDbContext context
        ) : base(context)
        {
            this.context = context;
        }

        public async Task<TurnoDTO?> SelectPorId(int id)
        {
            return await context.Turnos
                .Include(t => t.SerieTurno)
                .Include(t => t.TurnoProfesionales)
                    .ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes)
                    .ThenInclude(tp => tp.Pacientes)
                .Where(t => t.Id == id)
                .Select(t => new TurnoDTO
                {
                    Id = t.Id,

                    Fecha = t.FechaInicio
                        .ToLocalTime()
                        .Date,

                    Hora = TimeOnly.FromDateTime(
                        t.FechaInicio.ToLocalTime()
                    ),

                    FechaFin = t.FechaFin.ToLocalTime(),
                    EstadoTurno = t.EstadoTurno,
                    TipoTurnoId = t.TipoTurnoId ?? -1,
                    TipoConsultorioId = t.TipoConsultorioId,

                    DuracionPersonalizada =
                        t.TipoTurnoId == null
                            ? (int)(t.FechaFin - t.FechaInicio)
                                .TotalMinutes
                            : 0,

                    SerieTurnoId = t.SerieTurnoId,

                    FrecuenciaRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.Frecuencia
                            : EnumFrecuenciaRecurrenciaTurno
                                .noRepite,

                    IntervaloRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.Intervalo
                            : 1,

                    UnidadRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.UnidadPersonalizada
                            : null,

                    FechaHastaRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.FechaHasta
                                .ToLocalTime()
                            : null,

                    // Compatibilidad con el frontend anterior.
                    ProfesionalId = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .FirstOrDefault(),

                    NombreProfesional = t.TurnoProfesionales
                        .Select(tp =>
                            tp.Profesionales!.Nombre
                        )
                        .FirstOrDefault(),

                    PacienteId = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
                        .FirstOrDefault(),

                    NombrePaciente = t.TurnoPacientes
                        .Select(tp =>
                            tp.Pacientes!.Nombre
                        )
                        .FirstOrDefault(),

                    ProfesionalIds = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .ToList(),

                    PacienteIds = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TurnoListadoDTO>>
            SelectListaTurnos()
        {
            return await context.Turnos
                .Include(t => t.TipoTurnos)
                .Include(t => t.TipoConsultorios)
                .Include(t => t.SerieTurno)
                .Include(t => t.TurnoProfesionales)
                    .ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes)
                    .ThenInclude(tp => tp.Pacientes)
                .Where(t =>
                    t.EstadoRegistro ==
                    EnumEstadoRegistro.activo
                )
                .Select(t => new TurnoListadoDTO
                {
                    Id = t.Id,
                    FechaInicio =
                        t.FechaInicio.ToLocalTime(),
                    FechaFin =
                        t.FechaFin.ToLocalTime(),
                    EstadoTurno = t.EstadoTurno,

                    TipoTurnoId =
                        t.TipoTurnoId ?? -1,

                    NombreTipoTurno = t.TipoTurnos != null
                     ? t.TipoTurnos.Tipo
                        : $"Otro ({(int)(t.FechaFin - t.FechaInicio).TotalMinutes} min)",

                    TipoConsultorioId =
                        t.TipoConsultorioId,

                    NombreTipoConsultorio =
                        t.TipoConsultorios != null
                            ? t.TipoConsultorios.Tipo
                            : string.Empty,

                    SerieTurnoId = t.SerieTurnoId,

                    FrecuenciaRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.Frecuencia
                            : EnumFrecuenciaRecurrenciaTurno
                                .noRepite,

                    IntervaloRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.Intervalo
                            : 1,

                    UnidadRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno
                                .UnidadPersonalizada
                            : null,

                    FechaHastaRecurrencia =
                        t.SerieTurno != null
                            ? t.SerieTurno.FechaHasta
                                .ToLocalTime()
                            : null,

                    // Compatibilidad con el frontend anterior.
                    ProfesionalId = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .FirstOrDefault(),

                    NombreProfesional =
                        t.TurnoProfesionales
                            .Select(tp =>
                                tp.Profesionales!.Nombre
                            )
                            .FirstOrDefault(),

                    PacienteId = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
                        .FirstOrDefault(),

                    NombrePaciente =
                        t.TurnoPacientes
                            .Select(tp =>
                                tp.Pacientes!.Nombre
                            )
                            .FirstOrDefault(),

                    ProfesionalIds =
                        t.TurnoProfesionales
                            .OrderBy(tp =>
                                tp.Profesionales!.Nombre
                            )
                            .Select(tp =>
                                tp.ProfesionalId
                            )
                            .ToList(),

                    NombresProfesionales =
                        t.TurnoProfesionales
                            .OrderBy(tp =>
                                tp.Profesionales!.Nombre
                            )
                            .Select(tp =>
                                tp.Profesionales!.Nombre
                            )
                            .ToList(),

                    PacienteIds =
                        t.TurnoPacientes
                            .OrderBy(tp =>
                                tp.Pacientes!.Nombre
                            )
                            .Select(tp =>
                                tp.PacienteId
                            )
                            .ToList(),

                    NombresPacientes =
                        t.TurnoPacientes
                            .OrderBy(tp =>
                                tp.Pacientes!.Nombre
                            )
                            .Select(tp =>
                                tp.Pacientes!.Nombre
                            )
                            .ToList()
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTurno(
            TurnoDTO dto
        )
        {
            if (dto.Hora == TimeOnly.MinValue)
            {
                throw new ApplicationException(
                    "Debe seleccionar una hora válida."
                );
            }

            if (dto.TipoTurnoId == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un tipo de turno válido o 'Otro'."
                );
            }

            if (dto.TipoConsultorioId <= 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un consultorio válido."
                );
            }

            var profesionalIds = dto.ProfesionalIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var pacienteIds = dto.PacienteIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // Compatibilidad temporal con el frontend anterior.
            if (profesionalIds.Count == 0 &&
                dto.ProfesionalId > 0)
            {
                profesionalIds.Add(
                    dto.ProfesionalId
                );
            }

            if (pacienteIds.Count == 0 &&
                dto.PacienteId > 0)
            {
                pacienteIds.Add(
                    dto.PacienteId
                );
            }

            if (profesionalIds.Count == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar al menos un profesional."
                );
            }

            if (pacienteIds.Count == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar al menos un paciente."
                );
            }

            var fechasRecurrencia =
                CalculadorRecurrenciaTurno
                    .CalcularFechas(
                        dto.Fecha,
                        dto.FrecuenciaRecurrencia,
                        dto.FechaHastaRecurrencia,
                        dto.IntervaloRecurrencia,
                        dto.UnidadRecurrencia
                    );

            var fechasInicioUtc = fechasRecurrencia
                .Select(fecha =>
                    DateTime.SpecifyKind(
                        DateOnly
                            .FromDateTime(fecha)
                            .ToDateTime(dto.Hora),
                        DateTimeKind.Utc
                    )
                )
                .ToList();

            var fechasConConflicto =
                await context.Turnos
                    .Where(turno =>
                        turno.EstadoRegistro ==
                            EnumEstadoRegistro.activo &&
                        turno.TipoConsultorioId ==
                            dto.TipoConsultorioId &&
                        fechasInicioUtc.Contains(
                            turno.FechaInicio
                        )
                    )
                    .Select(turno =>
                        turno.FechaInicio
                    )
                    .OrderBy(fecha => fecha)
                    .ToListAsync();

            if (fechasConConflicto.Count > 0)
            {
                var fechasTexto = string.Join(
                    ", ",
                    fechasConConflicto.Select(fecha =>
                        fecha.ToString(
                            "dd/MM/yyyy HH:mm"
                        )
                    )
                );

                throw new ApplicationException(
                    "No se puede crear la serie porque " +
                    "ya existen turnos en estos horarios: " +
                    $"{fechasTexto}."
                );
            }

            int duracionMinutos;

            int? tipoTurnoId =
                dto.TipoTurnoId == -1
                    ? null
                    : dto.TipoTurnoId;

            if (tipoTurnoId is null)
            {
                if (dto.DuracionPersonalizada <= 0)
                {
                    throw new ApplicationException(
                        "Debe indicar una duración personalizada válida."
                    );
                }

                duracionMinutos =
                    dto.DuracionPersonalizada;
            }
            else
            {
                var tipoTurno =
                    await context.TipoTurnos
                        .FirstOrDefaultAsync(tipo =>
                            tipo.Id == tipoTurnoId
                        );

                if (tipoTurno is null)
                {
                    throw new ApplicationException(
                        "No existe el tipo de turno " +
                        $"con id {dto.TipoTurnoId}."
                    );
                }

                duracionMinutos =
                    tipoTurno.DuracionMinutos;
            }

            await using var transaccion =
                await context.Database
                    .BeginTransactionAsync();

            try
            {
                SerieTurno? serieTurno = null;

                var esRecurrente =
                    dto.FrecuenciaRecurrencia !=
                    EnumFrecuenciaRecurrenciaTurno
                        .noRepite;

                if (esRecurrente)
                {
                    serieTurno = new SerieTurno
                    {
                        Frecuencia =
                            dto.FrecuenciaRecurrencia,

                        Intervalo =
                            dto.IntervaloRecurrencia,

                        UnidadPersonalizada =
                            dto.FrecuenciaRecurrencia ==
                            EnumFrecuenciaRecurrenciaTurno
                                .personalizado
                                ? dto.UnidadRecurrencia
                                : null,

                        FechaInicio =
                            DateTime.SpecifyKind(
                                dto.Fecha.Date,
                                DateTimeKind.Utc
                            ),

                        FechaHasta =
                            DateTime.SpecifyKind(
                                dto.FechaHastaRecurrencia!
                                    .Value
                                    .Date,
                                DateTimeKind.Utc
                            ),

                        EstadoRegistro =
                            EnumEstadoRegistro.activo
                    };

                    context.SeriesTurnos.Add(
                        serieTurno
                    );

                    await context.SaveChangesAsync();
                }

                var turnos = fechasInicioUtc
                    .Select(fechaInicio =>
                        new Turno
                        {
                            FechaInicio =
                                fechaInicio,

                            FechaFin =
                                fechaInicio.AddMinutes(
                                    duracionMinutos
                                ),

                            EstadoTurno =
                                dto.EstadoTurno,

                            TipoTurnoId =
                                tipoTurnoId,

                            TipoConsultorioId =
                                dto.TipoConsultorioId,

                            SerieTurnoId =
                                serieTurno?.Id,

                            EstadoRegistro =
                                EnumEstadoRegistro.activo
                        }
                    )
                    .ToList();

                context.Turnos.AddRange(turnos);

                // Guardamos para obtener los Id.
                await context.SaveChangesAsync();

                var relacionesProfesionales =
                    turnos
                        .SelectMany(turno =>
                            profesionalIds.Select(
                                profesionalId =>
                                    new TurnoProfesional
                                    {
                                        TurnoId =
                                            turno.Id,

                                        ProfesionalId =
                                            profesionalId
                                    }
                            )
                        )
                        .ToList();

                var relacionesPacientes =
                    turnos
                        .SelectMany(turno =>
                            pacienteIds.Select(
                                pacienteId =>
                                    new TurnoPaciente
                                    {
                                        TurnoId =
                                            turno.Id,

                                        PacienteId =
                                            pacienteId
                                    }
                            )
                        )
                        .ToList();

                context.AddRange(
                    relacionesProfesionales
                );

                context.AddRange(
                    relacionesPacientes
                );

                await context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return turnos.First().Id;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task<List<TimeOnly>>
            HorariosDisponibles(
                DateOnly fecha,
                int tipoTurnoId,
                int consultorioId
            )
        {
            var tipoTurno =
                await context.TipoTurnos
                    .FirstOrDefaultAsync(tipo =>
                        tipo.Id == tipoTurnoId
                    );

            if (tipoTurno is null)
            {
                throw new ApplicationException(
                    "Tipo de turno inválido."
                );
            }

            var duracion =
                tipoTurno.DuracionMinutos;

            var inicioDia =
                new TimeOnly(8, 0);

            var finDia =
                new TimeOnly(20, 0);

            var horarios =
                new List<TimeOnly>();

            var horaActual = inicioDia;

            while (
                horaActual.AddMinutes(duracion) <=
                finDia
            )
            {
                var fechaHoraUtc =
                    DateTime.SpecifyKind(
                        fecha.ToDateTime(horaActual),
                        DateTimeKind.Utc
                    );

                var ocupado =
                    await context.Turnos.AnyAsync(
                        turno =>
                            turno.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            turno.TipoConsultorioId ==
                                consultorioId &&
                            turno.FechaInicio ==
                                fechaHoraUtc
                    );

                if (!ocupado)
                {
                    horarios.Add(horaActual);
                }

                horaActual =
                    horaActual.AddMinutes(duracion);
            }

            return horarios;
        }

        public async Task<bool> ActualizarTurno(
            int id,
            TurnoDTO dto
        )
        {
            if (dto.Hora == TimeOnly.MinValue)
            {
                throw new ApplicationException(
                    "Debe seleccionar una hora válida."
                );
            }

            if (dto.TipoTurnoId == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un tipo de turno válido o 'Otro'."
                );
            }

            if (dto.TipoConsultorioId <= 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un consultorio válido."
                );
            }

            var profesionalIds = dto.ProfesionalIds
                .Where(profesionalId =>
                    profesionalId > 0
                )
                .Distinct()
                .ToList();

            var pacienteIds = dto.PacienteIds
                .Where(pacienteId =>
                    pacienteId > 0
                )
                .Distinct()
                .ToList();

            // Compatibilidad temporal con el frontend anterior.
            if (profesionalIds.Count == 0 &&
                dto.ProfesionalId > 0)
            {
                profesionalIds.Add(
                    dto.ProfesionalId
                );
            }

            if (pacienteIds.Count == 0 &&
                dto.PacienteId > 0)
            {
                pacienteIds.Add(
                    dto.PacienteId
                );
            }

            if (profesionalIds.Count == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar al menos un profesional."
                );
            }

            if (pacienteIds.Count == 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar al menos un paciente."
                );
            }

            var turno = await context.Turnos
                .Include(t =>
                    t.TurnoProfesionales
                )
                .Include(t =>
                    t.TurnoPacientes
                )
                .FirstOrDefaultAsync(t =>
                    t.Id == id
                );

            if (turno is null)
            {
                return false;
            }

            int duracionMinutos;

            int? tipoTurnoId =
                dto.TipoTurnoId == -1
                    ? null
                    : dto.TipoTurnoId;

            if (tipoTurnoId is null)
            {
                if (dto.DuracionPersonalizada <= 0)
                {
                    throw new ApplicationException(
                        "Debe indicar una duración personalizada válida."
                    );
                }

                duracionMinutos =
                    dto.DuracionPersonalizada;
            }
            else
            {
                var tipoTurno =
                    await context.TipoTurnos
                        .FirstOrDefaultAsync(tipo =>
                            tipo.Id == tipoTurnoId
                        );

                if (tipoTurno is null)
                {
                    throw new ApplicationException(
                        "No existe el tipo de turno " +
                        $"con id {dto.TipoTurnoId}."
                    );
                }

                duracionMinutos =
                    tipoTurno.DuracionMinutos;
            }

            // Si eligió toda la serie, usamos el método nuevo.
            // La fecha enviada por el formulario no se aplica.
            if (dto.ModificarTodaLaSerie)
            {
                return await ActualizarTodaLaSerie(
                    turno,
                    dto,
                    profesionalIds,
                    pacienteIds,
                    tipoTurnoId,
                    duracionMinutos
                );
            }

            // Desde acá continúa la modificación de un solo turno.
            var fecha =
                DateOnly.FromDateTime(dto.Fecha);

            var fechaInicioUtc =
                DateTime.SpecifyKind(
                    fecha.ToDateTime(dto.Hora),
                    DateTimeKind.Utc
                );

            if (fechaInicioUtc.Date < DateTime.UtcNow.Date)
            {
                throw new ApplicationException(
                    "No se pueden mover turnos a días pasados."
                );
            }

            var existe = await context.Turnos
                .AnyAsync(otroTurno =>
                    otroTurno.EstadoRegistro ==
                        EnumEstadoRegistro.activo &&
                    otroTurno.TipoConsultorioId ==
                        dto.TipoConsultorioId &&
                    otroTurno.FechaInicio ==
                        fechaInicioUtc &&
                    otroTurno.Id != id
                );

            if (existe)
            {
                throw new ApplicationException(
                    "Ya existe un turno en ese consultorio, " +
                    "fecha y hora."
                );
            }

            var fechaFinUtc =
                fechaInicioUtc.AddMinutes(
                    duracionMinutos
                );

            turno.FechaInicio = fechaInicioUtc;
            turno.FechaFin = fechaFinUtc;
            turno.EstadoTurno = dto.EstadoTurno;
            turno.TipoTurnoId = tipoTurnoId;
            turno.TipoConsultorioId =
                dto.TipoConsultorioId;

            context.RemoveRange(
                turno.TurnoProfesionales
            );

            context.RemoveRange(
                turno.TurnoPacientes
            );

            var nuevosTurnoProfesionales =
                profesionalIds
                    .Select(profesionalId =>
                        new TurnoProfesional
                        {
                            TurnoId = turno.Id,
                            ProfesionalId =
                                profesionalId
                        }
                    )
                    .ToList();

            var nuevosTurnoPacientes =
                pacienteIds
                    .Select(pacienteId =>
                        new TurnoPaciente
                        {
                            TurnoId = turno.Id,
                            PacienteId = pacienteId
                        }
                    )
                    .ToList();

            context.AddRange(
                nuevosTurnoProfesionales
            );

            context.AddRange(
                nuevosTurnoPacientes
            );

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTurno(int id)
        {
            var turno =
                await context.Turnos
                    .FirstOrDefaultAsync(t =>
                        t.Id == id
                    );

            if (turno is null)
            {
                return false;
            }

            turno.EstadoRegistro =
                EnumEstadoRegistro.inactivo;

            await context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> ActualizarTodaLaSerie(
    Turno turnoSeleccionado,
    TurnoDTO dto,
    List<int> profesionalIds,
    List<int> pacienteIds,
    int? tipoTurnoId,
    int duracionMinutos
)
        {
            if (!turnoSeleccionado.SerieTurnoId.HasValue)
            {
                throw new ApplicationException(
                    "El turno seleccionado no pertenece a una serie."
                );
            }

            var serieTurnoId =
                turnoSeleccionado.SerieTurnoId.Value;

            var turnosSerie = await context.Turnos
                .Include(turno => turno.TurnoProfesionales)
                .Include(turno => turno.TurnoPacientes)
                .Where(turno =>
                    turno.SerieTurnoId == serieTurnoId &&
                    turno.EstadoRegistro ==
                        EnumEstadoRegistro.activo
                )
                .OrderBy(turno => turno.FechaInicio)
                .ToListAsync();

            if (turnosSerie.Count == 0)
            {
                throw new ApplicationException(
                    "No se encontraron turnos activos en la serie."
                );
            }

            var turnoIds = turnosSerie
                .Select(turno => turno.Id)
                .ToList();

            var nuevasFechasInicio = turnosSerie
                .Select(turno =>
                    DateTime.SpecifyKind(
                        DateOnly
                            .FromDateTime(turno.FechaInicio)
                            .ToDateTime(dto.Hora),
                        DateTimeKind.Utc
                    )
                )
                .ToList();

            var conflictos = await context.Turnos
                .Where(turno =>
                    turno.EstadoRegistro ==
                        EnumEstadoRegistro.activo &&
                    turno.TipoConsultorioId ==
                        dto.TipoConsultorioId &&
                    !turnoIds.Contains(turno.Id) &&
                    nuevasFechasInicio.Contains(
                        turno.FechaInicio
                    )
                )
                .Select(turno => turno.FechaInicio)
                .OrderBy(fecha => fecha)
                .ToListAsync();

            if (conflictos.Count > 0)
            {
                var conflictosTexto = string.Join(
                    ", ",
                    conflictos.Select(fecha =>
                        fecha
                            .ToLocalTime()
                            .ToString("dd/MM/yyyy HH:mm")
                    )
                );

                throw new ApplicationException(
                    "No se puede actualizar toda la serie " +
                    "porque existen conflictos en estos horarios: " +
                    $"{conflictosTexto}."
                );
            }

            await using var transaccion =
                await context.Database
                    .BeginTransactionAsync();

            try
            {
                foreach (var turno in turnosSerie)
                {
                    var fechaTurno =
                        DateOnly.FromDateTime(
                            turno.FechaInicio
                        );

                    var nuevaFechaInicio =
                        DateTime.SpecifyKind(
                            fechaTurno.ToDateTime(dto.Hora),
                            DateTimeKind.Utc
                        );

                    turno.FechaInicio =
                        nuevaFechaInicio;

                    turno.FechaFin =
                        nuevaFechaInicio.AddMinutes(
                            duracionMinutos
                        );

                    turno.EstadoTurno =
                        dto.EstadoTurno;

                    turno.TipoTurnoId =
                        tipoTurnoId;

                    turno.TipoConsultorioId =
                        dto.TipoConsultorioId;

                    context.RemoveRange(
                        turno.TurnoProfesionales
                    );

                    context.RemoveRange(
                        turno.TurnoPacientes
                    );
                }

                await context.SaveChangesAsync();

                var nuevasRelacionesProfesionales =
                    turnosSerie
                        .SelectMany(turno =>
                            profesionalIds.Select(
                                profesionalId =>
                                    new TurnoProfesional
                                    {
                                        TurnoId = turno.Id,

                                        ProfesionalId =
                                            profesionalId
                                    }
                            )
                        )
                        .ToList();

                var nuevasRelacionesPacientes =
                    turnosSerie
                        .SelectMany(turno =>
                            pacienteIds.Select(
                                pacienteId =>
                                    new TurnoPaciente
                                    {
                                        TurnoId = turno.Id,

                                        PacienteId =
                                            pacienteId
                                    }
                            )
                        )
                        .ToList();

                context.AddRange(
                    nuevasRelacionesProfesionales
                );

                context.AddRange(
                    nuevasRelacionesPacientes
                );

                await context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return true;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
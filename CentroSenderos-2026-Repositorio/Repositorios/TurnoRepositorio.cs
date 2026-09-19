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

        public TurnoRepositorio(ApplicationDbContext context) : base(context)
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
                .Include(t => t.TurnoTipoPrestaciones)
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

                    TipoTurnoId = t.TipoTurnoId,

                    TipoConsultorioId = t.TipoConsultorioId,

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
                        .ToList(),

                    TipoPrestacionIds = t.TurnoTipoPrestaciones
                        .Where(relacion =>
                            relacion.EstadoRegistro ==
                            EnumEstadoRegistro.activo
                        )
                        .Select(relacion =>
                            relacion.TipoPrestacionId
                        )
                        .ToList(),

                    ProfesionalPrestaciones = t.TurnoTipoPrestaciones
                            .Where(relacion =>
                                relacion.EstadoRegistro ==
                                EnumEstadoRegistro.activo
                            )
                            .Select(relacion =>
                                new TurnoProfesionalPrestacionDTO
                                {
                                    ProfesionalId =
                                        relacion.ProfesionalId,

                                    NombreProfesional =
                                        relacion.Profesionales != null
                                            ? relacion.Profesionales.Nombre
                                            : "Sin profesional",

                                    TipoPrestacionId =
                                        relacion.TipoPrestacionId,

                                    NombreTipoPrestacion =
                                        relacion.TipoPrestaciones != null
                                            ? relacion.TipoPrestaciones.Tipo
                                            : "Sin prestación"
                                }
                            )
                         .ToList(),

                    PacientesDetalle = t.TurnoPacientes
                        .OrderBy(relacion =>
                            relacion.Pacientes!.Nombre
                        )
                        .Select(relacion =>
                            new TurnoPacienteDetalleDTO
                            {
                                PacienteId =
                                    relacion.PacienteId,

                                NombrePaciente =
                                    relacion.Pacientes != null
                                        ? relacion.Pacientes.Nombre
                                        : "Paciente",

                                TipoObraSocialId =
                                    relacion.TipoObraSocialId,

                                NombreObraSocial =
                                    relacion.NombreObraSocial
                            }
                        )
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TurnoListadoDTO>> SelectListaTurnos()
        {
            return await context.Turnos
                .Include(t => t.TipoTurnos)
                .Include(t => t.TipoConsultorios)
                .Include(t => t.SerieTurno)
                .Include(t => t.TurnoProfesionales)
                    .ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes)
                    .ThenInclude(tp => tp.Pacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                    .ThenInclude(tp => tp.TipoPrestaciones)
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

                    TipoTurnoId = t.TipoTurnoId,

                    NombreTipoTurno =
                        t.TipoTurnos != null
                            ? t.TipoTurnos.Tipo
                            : "Sin tipo",

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
                            : EnumFrecuenciaRecurrenciaTurno.noRepite,

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
                            ? t.SerieTurno.FechaHasta.ToLocalTime()
                            : null,

                    // Compatibilidad con el frontend anterior.
                    ProfesionalId =
                        t.TurnoProfesionales
                            .Select(tp => tp.ProfesionalId)
                            .FirstOrDefault(),

                    NombreProfesional =
                        t.TurnoProfesionales
                            .Select(tp =>
                                tp.Profesionales!.Nombre
                            )
                            .FirstOrDefault(),

                    PacienteId =
                        t.TurnoPacientes
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
                            .ToList(),

                    TipoPrestacionIds =
                        t.TurnoTipoPrestaciones
                            .Where(relacion =>
                                relacion.EstadoRegistro ==
                                EnumEstadoRegistro.activo
                            )
                            .OrderBy(relacion =>
                                relacion.TipoPrestaciones!.Tipo
                            )
                            .Select(relacion =>
                                relacion.TipoPrestacionId
                            )
                            .ToList(),

                    NombresTipoPrestaciones =
                        t.TurnoTipoPrestaciones
                            .Where(relacion =>
                                relacion.EstadoRegistro ==
                                EnumEstadoRegistro.activo
                            )
                            .OrderBy(relacion =>
                                relacion.TipoPrestaciones!.Tipo
                            )
                            .Select(relacion =>
                                relacion.TipoPrestaciones!.Tipo
                            )
                            .ToList(),

                    // La prestación y el profesional se proyectan juntos.
                    // No deben reconstruirse cruzando dos listas separadas.
                    ProfesionalPrestaciones =
                        t.TurnoTipoPrestaciones
                            .Where(relacion =>
                                relacion.EstadoRegistro ==
                                EnumEstadoRegistro.activo
                            )
                            .OrderBy(relacion =>
                                relacion.Profesionales!.Nombre
                            )
                            .Select(relacion =>
                                new TurnoProfesionalPrestacionDTO
                                {
                                    ProfesionalId =
                                        relacion.ProfesionalId,

                                    NombreProfesional =
                                        relacion.Profesionales != null
                                            ? relacion.Profesionales.Nombre
                                            : "Sin profesional",

                                    TipoPrestacionId =
                                        relacion.TipoPrestacionId,

                                    NombreTipoPrestacion =
                                        relacion.TipoPrestaciones != null
                                            ? relacion.TipoPrestaciones.Tipo
                                            : "Sin prestación"
                                }
                            )
                            .ToList(),

                    PacientesDetalle =
                        t.TurnoPacientes
                            .OrderBy(relacion =>
                                relacion.Pacientes!.Nombre
                            )
                            .Select(relacion =>
                                new TurnoPacienteDetalleDTO
                                {
                                    PacienteId =
                                        relacion.PacienteId,

                                    NombrePaciente =
                                        relacion.Pacientes != null
                                            ? relacion.Pacientes.Nombre
                                            : "Paciente",

                                    TipoObraSocialId =
                                        relacion.TipoObraSocialId,

                                    NombreObraSocial =
                                        relacion.NombreObraSocial
                                }
                            )
                            .ToList()
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTurno(TurnoDTO dto)
        {
            if (dto.Hora == TimeOnly.MinValue)
            {
                throw new ApplicationException(
                    "Debe seleccionar una hora válida."
                );
            }

            if (dto.TipoTurnoId <= 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un tipo de turno válido."
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

            var profesionalPrestaciones =
                dto.ProfesionalPrestaciones
                .Where(seleccion =>
                    seleccion.ProfesionalId > 0 &&
                    seleccion.TipoPrestacionId > 0
                )
                .GroupBy(seleccion =>
                    seleccion.ProfesionalId
                )
                .Select(grupo =>
                    grupo.First()
                )
                .ToList();

            var tipoPrestacionIds =
                profesionalPrestaciones
                    .Select(seleccion =>
                        seleccion.TipoPrestacionId
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

            if (profesionalPrestaciones.Count != profesionalIds.Count)
            {
                throw new ApplicationException(
                    "Debe seleccionar una prestación para cada profesional."
                );
            }

            var profesionalesConPrestacion =
                profesionalPrestaciones
                    .Select(seleccion =>
                        seleccion.ProfesionalId
                    )
                    .OrderBy(id => id)
                    .ToList();

            var profesionalesSeleccionados =
                profesionalIds
                    .OrderBy(id => id)
                    .ToList();

            if (!profesionalesConPrestacion.SequenceEqual(
                    profesionalesSeleccionados
                ))
            {
                throw new ApplicationException(
                    "Las prestaciones seleccionadas no coinciden con los profesionales del turno."
                );
            }

            var relacionesProfesionalPrestacion =
    await context.ProfesionalTipoPrestaciones
        .Where(relacion =>
            profesionalIds.Contains(
                relacion.ProfesionalId
            ) &&
            tipoPrestacionIds.Contains(
                relacion.TipoPrestacionId
            ) &&
            relacion.EstadoRegistro ==
                EnumEstadoRegistro.activo &&
            relacion.Profesional.EstadoRegistro ==
                EnumEstadoRegistro.activo &&
            relacion.TipoPrestacion.EstadoRegistro ==
                EnumEstadoRegistro.activo
        )
        .Select(relacion => new
        {
            relacion.ProfesionalId,
            relacion.TipoPrestacionId
        })
        .ToListAsync();

            var relacionesValidas =
                relacionesProfesionalPrestacion
                    .Select(relacion => (
                        relacion.ProfesionalId,
                        relacion.TipoPrestacionId
                    ))
                    .ToHashSet();

            var existeSeleccionInvalida =
                profesionalPrestaciones.Any(seleccion =>
                    !relacionesValidas.Contains((
                        seleccion.ProfesionalId,
                        seleccion.TipoPrestacionId
                    ))
                );

            if (existeSeleccionInvalida)
            {
                throw new ApplicationException(
                    "La prestación seleccionada no pertenece al profesional indicado."
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

            var tipoTurno = await context.TipoTurnos.FirstOrDefaultAsync(tipo => tipo.Id == dto.TipoTurnoId);

            if (tipoTurno is null)
            {
                throw new ApplicationException(
                    "No existe el tipo de turno " +
                    $"con id {dto.TipoTurnoId}."
                );
            }

            var tipoTurnoId = tipoTurno.Id;
            var duracionMinutos =
                tipoTurno.DuracionMinutos;

            // Consultamos solamente el período comprendido
            // entre la primera y la última ocurrencia.
            var primeraFechaInicio =
                fechasInicioUtc.Min();

            var ultimaFechaFin =
                fechasInicioUtc
                    .Max()
                    .AddMinutes(duracionMinutos);
            var turnosDelConsultorio =
    await context.Turnos
        .Where(turno =>
            turno.EstadoRegistro ==
                EnumEstadoRegistro.activo &&
            turno.EstadoTurno !=
                EnumEstadoTurno.cancelado &&
            turno.TipoConsultorioId ==
                dto.TipoConsultorioId &&
            turno.FechaInicio < ultimaFechaFin &&
            turno.FechaFin > primeraFechaInicio
        )
        .Select(turno => new
        {
            turno.FechaInicio,
            turno.FechaFin,

            NombreConsultorio =
                turno.TipoConsultorios != null
                    ? turno.TipoConsultorios.Tipo
                    : "Consultorio"
        })
        .ToListAsync();

            var conflictosConsultorio =
                turnosDelConsultorio
                    .Where(turnoExistente =>
                        fechasInicioUtc.Any(nuevaFechaInicio =>
                            turnoExistente.FechaInicio <
                                nuevaFechaInicio.AddMinutes(
                                    duracionMinutos
                                ) &&
                            turnoExistente.FechaFin >
                                nuevaFechaInicio
                        )
                    )
                    .ToList();

            if (conflictosConsultorio.Count > 0)
            {
                var nombreConsultorio =
                    conflictosConsultorio[0].NombreConsultorio;

                var horariosTexto = string.Join(
                    ", ",
                    conflictosConsultorio.Select(conflicto =>
                        $"{conflicto.FechaInicio:dd/MM/yyyy} " +
                        $"de {conflicto.FechaInicio:HH:mm} " +
                        $"a {conflicto.FechaFin:HH:mm}"
                    )
                );

                throw new ApplicationException(
                    $"El consultorio \"{nombreConsultorio}\" " +
                    $"ya está ocupado en estos horarios: " +
                    $"{horariosTexto}."
                );
            }

            var turnosDeProfesionales =
    await context.Turnos
        .Where(turno =>
            turno.EstadoRegistro ==
                EnumEstadoRegistro.activo &&
            turno.EstadoTurno !=
                EnumEstadoTurno.cancelado &&
            turno.FechaInicio < ultimaFechaFin &&
            turno.FechaFin > primeraFechaInicio
        )
        .SelectMany(
            turno => turno.TurnoProfesionales
                .Where(relacion =>
                    profesionalIds.Contains(
                        relacion.ProfesionalId
                    )
                ),
            (turno, relacion) => new
            {
                turno.FechaInicio,
                turno.FechaFin,

                NombreProfesional =
                    relacion.Profesionales != null
                        ? relacion.Profesionales.Nombre
                        : "Profesional"
            }
        )
        .ToListAsync();

            var conflictosProfesionales =
                turnosDeProfesionales
                    .Where(turnoExistente =>
                        fechasInicioUtc.Any(nuevaFechaInicio =>
                            turnoExistente.FechaInicio <
                                nuevaFechaInicio.AddMinutes(
                                    duracionMinutos
                                ) &&
                            turnoExistente.FechaFin >
                                nuevaFechaInicio
                        )
                    )
                    .ToList();

            if (conflictosProfesionales.Count > 0)
            {
                var conflictosTexto = string.Join(
                    ", ",
                    conflictosProfesionales.Select(conflicto =>
                        $"\"{conflicto.NombreProfesional}\" " +
                        $"el {conflicto.FechaInicio:dd/MM/yyyy} " +
                        $"de {conflicto.FechaInicio:HH:mm} " +
                        $"a {conflicto.FechaFin:HH:mm}"
                    )
                );

                var inicioMensaje =
                    conflictosProfesionales.Count == 1
                        ? "El profesional"
                        : "Los profesionales";

                throw new ApplicationException(
                    $"{inicioMensaje} {conflictosTexto} " +
                    "ya tiene un turno en ese horario."
                );
            }

            var turnosDePacientes =
    await context.Turnos
        .Where(turno =>
            turno.EstadoRegistro ==
                EnumEstadoRegistro.activo &&
            turno.EstadoTurno !=
                EnumEstadoTurno.cancelado &&
            turno.FechaInicio < ultimaFechaFin &&
            turno.FechaFin > primeraFechaInicio
        )
        .SelectMany(
            turno => turno.TurnoPacientes
                .Where(relacion =>
                    pacienteIds.Contains(
                        relacion.PacienteId
                    )
                ),
            (turno, relacion) => new
            {
                turno.FechaInicio,
                turno.FechaFin,

                NombrePaciente =
                    relacion.Pacientes != null
                        ? relacion.Pacientes.Nombre
                        : "Paciente"
            }
        )
        .ToListAsync();

            var conflictosPacientes =
                turnosDePacientes
                    .Where(turnoExistente =>
                        fechasInicioUtc.Any(nuevaFechaInicio =>
                            turnoExistente.FechaInicio <
                                nuevaFechaInicio.AddMinutes(
                                    duracionMinutos
                                ) &&
                            turnoExistente.FechaFin >
                                nuevaFechaInicio
                        )
                    )
                    .ToList();

            if (conflictosPacientes.Count > 0)
            {
                var conflictosTexto = string.Join(
                    ", ",
                    conflictosPacientes.Select(conflicto =>
                        $"\"{conflicto.NombrePaciente}\" " +
                        $"el {conflicto.FechaInicio:dd/MM/yyyy} " +
                        $"de {conflicto.FechaInicio:HH:mm} " +
                        $"a {conflicto.FechaFin:HH:mm}"
                    )
                );

                var inicioMensaje =
                    conflictosPacientes.Count == 1
                        ? "El paciente"
                        : "Los pacientes";

                var verbo =
                    conflictosPacientes.Count == 1
                        ? "ya tiene"
                        : "ya tienen";

                throw new ApplicationException(
                    $"{inicioMensaje} {conflictosTexto} " +
                    $"{verbo} un turno en ese horario."
                );
            }

            var datosPacientes = await context.Pacientes
                .Where(paciente =>
                    pacienteIds.Contains(paciente.Id)
                )
                .Select(paciente => new
                {
                    paciente.Id,
                    paciente.TipoObraSocialId,

                    NombreObraSocial =
                        paciente.TipoObraSociales != null
                            ? paciente.TipoObraSociales.Tipo
                            : null
                })
                .ToDictionaryAsync(
                    paciente => paciente.Id
                );

            if (datosPacientes.Count != pacienteIds.Count)
            {
                throw new ApplicationException(
                    "Uno o más pacientes seleccionados no existen."
                );
            }





            if (existeSeleccionInvalida)
            {
                throw new ApplicationException(
                    "La prestación seleccionada no pertenece al profesional indicado."
                );
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
                {
                    var datosPaciente =
                        datosPacientes[pacienteId];

                    return new TurnoPaciente
                    {
                        TurnoId =
                            turno.Id,

                        PacienteId =
                            pacienteId,

                        TipoObraSocialId =
                            datosPaciente.TipoObraSocialId,

                        NombreObraSocial =
                            datosPaciente.NombreObraSocial,

                        EstadoRegistro =
                            EnumEstadoRegistro.activo
                    };
                }
            )
        )
        .ToList();

                var relacionesPrestaciones =
                    turnos
                        .SelectMany(turno =>
                            profesionalPrestaciones.Select(
                                seleccion =>
                                    new TurnoTipoPrestacion
                                    {
                                        TurnoId =
                                            turno.Id,

                                        ProfesionalId =
                                            seleccion.ProfesionalId,

                                        TipoPrestacionId =
                                            seleccion.TipoPrestacionId,

                                        EstadoRegistro =
                                            EnumEstadoRegistro.activo
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

                context.AddRange(
                    relacionesPrestaciones
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

        public async Task<List<TimeOnly>> HorariosDisponibles(DateOnly fecha, int tipoTurnoId, int consultorioId, List<int>? profesionalIds = null, List<int>? pacienteIds = null, int? turnoIdExcluir = null)
        {
            profesionalIds ??= new List<int>();
            pacienteIds ??= new List<int>();

            var tipoTurno = await context.TipoTurnos
                .FirstOrDefaultAsync(tipo =>
                    tipo.Id == tipoTurnoId
                );

            if (tipoTurno is null)
            {
                throw new ApplicationException(
                    "Tipo de turno inválido."
                );
            }

            var duracionMinutos =
                tipoTurno.DuracionMinutos;

            var horaInicioDia =
                new TimeOnly(8, 0);

            var horaFinDia =
                new TimeOnly(20, 0);

            var fechaInicioDiaUtc =
                DateTime.SpecifyKind(
                    fecha.ToDateTime(horaInicioDia),
                    DateTimeKind.Utc
                );

            var fechaFinDiaUtc =
                DateTime.SpecifyKind(
                    fecha.ToDateTime(horaFinDia),
                    DateTimeKind.Utc
                );

            var turnosQueBloquean =
                    await ConsultarTurnosQueBloquean(
                        consultorioId,
                        profesionalIds,
                        pacienteIds
                    )
                    .Where(turno =>
                        !turnoIdExcluir.HasValue ||
                        turno.Id != turnoIdExcluir.Value
                    )
                    .Where(turno =>
                        turno.FechaInicio < fechaFinDiaUtc &&
                        turno.FechaFin > fechaInicioDiaUtc
                    )
                    .Select(turno => new
                    {
                        turno.FechaInicio,
                        turno.FechaFin
                    })
                    .ToListAsync();

            var horariosDisponibles =
                new List<TimeOnly>();

            var horaActual = horaInicioDia;

            while (
                horaActual.AddMinutes(duracionMinutos) <=
                horaFinDia
            )
            {
                var fechaInicioTurnoUtc =
                    DateTime.SpecifyKind(
                        fecha.ToDateTime(horaActual),
                        DateTimeKind.Utc
                    );

                var fechaFinTurnoUtc =
                    fechaInicioTurnoUtc.AddMinutes(
                        duracionMinutos
                    );

                var ocupado = turnosQueBloquean.Any(
                    turnoExistente =>
                        turnoExistente.FechaInicio <
                            fechaFinTurnoUtc &&
                        turnoExistente.FechaFin >
                            fechaInicioTurnoUtc
                );

                if (!ocupado)
                {
                    horariosDisponibles.Add(
                        horaActual
                    );
                }

                horaActual =
                    horaActual.AddMinutes(
                        duracionMinutos
                    );
            }

            return horariosDisponibles;
        }

        public async Task<bool> ActualizarTurno(int id, TurnoDTO dto)
        {
            if (dto.Hora == TimeOnly.MinValue)
            {
                throw new ApplicationException(
                    "Debe seleccionar una hora válida."
                );
            }

            if (dto.TipoTurnoId <= 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un tipo de turno válido."
                );
            }

            if (dto.TipoConsultorioId <= 0)
            {
                throw new ApplicationException(
                    "Debe seleccionar un consultorio válido."
                );
            }

            var profesionalIds = dto.ProfesionalIds.Where(profesionalId => profesionalId > 0).Distinct().ToList();

            var pacienteIds = dto.PacienteIds.Where(pacienteId => pacienteId > 0).Distinct().ToList();

            var profesionalPrestaciones =
    dto.ProfesionalPrestaciones
        .Where(seleccion =>
            seleccion.ProfesionalId > 0 &&
            seleccion.TipoPrestacionId > 0
        )
        .GroupBy(seleccion =>
            seleccion.ProfesionalId
        )
        .Select(grupo =>
            grupo.First()
        )
        .ToList();

            var tipoPrestacionIds =
                profesionalPrestaciones
                    .Select(seleccion =>
                        seleccion.TipoPrestacionId
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

            if (profesionalPrestaciones.Count !=
    profesionalIds.Count)
            {
                throw new ApplicationException(
                    "Debe seleccionar una prestación para cada profesional."
                );
            }

            var profesionalesConPrestacion =
                profesionalPrestaciones
                    .Select(seleccion =>
                        seleccion.ProfesionalId
                    )
                    .OrderBy(profesionalId =>
                        profesionalId
                    )
                    .ToList();

            var profesionalesSeleccionados =
                profesionalIds
                    .OrderBy(profesionalId =>
                        profesionalId
                    )
                    .ToList();

            if (!profesionalesConPrestacion.SequenceEqual(
                    profesionalesSeleccionados
                ))
            {
                throw new ApplicationException(
                    "Las prestaciones seleccionadas no coinciden con los profesionales del turno."
                );
            }

            var relacionesProfesionalPrestacionActualizacion =
                await context.ProfesionalTipoPrestaciones
                    .Where(relacion =>
                        profesionalIds.Contains(relacion.ProfesionalId) &&
                        tipoPrestacionIds.Contains(relacion.TipoPrestacionId) &&
                        relacion.EstadoRegistro == EnumEstadoRegistro.activo &&
                        relacion.Profesional.EstadoRegistro == EnumEstadoRegistro.activo &&
                        relacion.TipoPrestacion.EstadoRegistro == EnumEstadoRegistro.activo
                    )
                    .Select(relacion => new
                    {
                        relacion.ProfesionalId,
                        relacion.TipoPrestacionId
                    })
                    .ToListAsync();

            var relacionesValidasActualizacion =
                relacionesProfesionalPrestacionActualizacion
                    .Select(relacion => (
                        relacion.ProfesionalId,
                        relacion.TipoPrestacionId
                    ))
                    .ToHashSet();

            var existeSeleccionInvalidaActualizacion =
                profesionalPrestaciones.Any(seleccion =>
                    !relacionesValidasActualizacion.Contains((
                        seleccion.ProfesionalId,
                        seleccion.TipoPrestacionId
                    ))
                );

            if (existeSeleccionInvalidaActualizacion)
            {
                throw new ApplicationException(
                    "La prestación seleccionada no pertenece al profesional indicado."
                );
            }

            var turno = await context.Turnos
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno is null)
            {
                return false;
            }

            var tipoTurno = await context.TipoTurnos.FirstOrDefaultAsync(tipo => tipo.Id == dto.TipoTurnoId);

            if (tipoTurno is null)
            {
                throw new ApplicationException(
                    "No existe el tipo de turno " +
                    $"con id {dto.TipoTurnoId}."
                );
            }

            var tipoTurnoId = tipoTurno.Id;
            var duracionMinutos =
                tipoTurno.DuracionMinutos;

            // Si eligió toda la serie, usamos el método nuevo.
            // La fecha enviada por el formulario no se aplica.
            if (dto.ModificarTodaLaSerie)
            {
                return await ActualizarTodaLaSerie(
                    turno,
                    dto,
                    profesionalIds,
                    pacienteIds,
                    profesionalPrestaciones,
                    tipoTurnoId,
                    duracionMinutos
                );
            }

            // Desde acá continúa la modificación de un solo turno.
            var fecha = DateOnly.FromDateTime(dto.Fecha);

            var fechaInicioUtc = DateTime.SpecifyKind(fecha.ToDateTime(dto.Hora), DateTimeKind.Utc);

            if (fechaInicioUtc.Date < DateTime.UtcNow.Date)
            {
                throw new ApplicationException("No se pueden mover turnos a días pasados.");
            }

            var fechaFinUtc = fechaInicioUtc.AddMinutes(duracionMinutos);

            var conflictoConsultorio =
    dto.EstadoTurno != EnumEstadoTurno.cancelado
        ? await context.Turnos
            .Where(otroTurno =>
                otroTurno.Id != id &&
                otroTurno.EstadoRegistro ==
                    EnumEstadoRegistro.activo &&
                otroTurno.EstadoTurno !=
                    EnumEstadoTurno.cancelado &&
                otroTurno.TipoConsultorioId ==
                    dto.TipoConsultorioId &&
                otroTurno.FechaInicio < fechaFinUtc &&
                otroTurno.FechaFin > fechaInicioUtc
            )
            .Select(otroTurno => new
            {
                otroTurno.FechaInicio,
                otroTurno.FechaFin,

                NombreConsultorio =
                    otroTurno.TipoConsultorios != null
                        ? otroTurno.TipoConsultorios.Tipo
                        : "Consultorio"
            })
            .FirstOrDefaultAsync()
        : null;

            if (conflictoConsultorio is not null)
            {
                var inicioConflicto = conflictoConsultorio.FechaInicio;

                var finConflicto = conflictoConsultorio.FechaFin;

                throw new ApplicationException(
                    $"El consultorio \"{conflictoConsultorio.NombreConsultorio}\" " +
                    $"ya está ocupado el {inicioConflicto:dd/MM/yyyy} " +
                    $"de {inicioConflicto:HH:mm} a {finConflicto:HH:mm}."
                );
            }

            var conflictoProfesional =
    dto.EstadoTurno != EnumEstadoTurno.cancelado
        ? await context.Turnos
            .Where(otroTurno =>
                otroTurno.Id != id &&
                otroTurno.EstadoRegistro ==
                    EnumEstadoRegistro.activo &&
                otroTurno.EstadoTurno !=
                    EnumEstadoTurno.cancelado &&
                otroTurno.FechaInicio < fechaFinUtc &&
                otroTurno.FechaFin > fechaInicioUtc &&
                otroTurno.TurnoProfesionales.Any(
                    relacion =>
                        profesionalIds.Contains(
                            relacion.ProfesionalId
                        )
                )
            )
            .Select(otroTurno => new
            {
                otroTurno.FechaInicio,
                otroTurno.FechaFin,

                NombreProfesional =
                    otroTurno.TurnoProfesionales
                        .Where(relacion =>
                            profesionalIds.Contains(
                                relacion.ProfesionalId
                            )
                        )
                        .Select(relacion =>
                            relacion.Profesionales != null
                                ? relacion.Profesionales.Nombre
                                : null
                        )
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync()
        : null;

            if (conflictoProfesional is not null)
            {
                var inicioConflicto =
                    conflictoProfesional.FechaInicio;

                var finConflicto =
                    conflictoProfesional.FechaFin;

                var nombreProfesional =
                    conflictoProfesional.NombreProfesional
                    ?? "Profesional seleccionado";

                throw new ApplicationException(
                    $"El profesional \"{nombreProfesional}\" " +
                    $"ya tiene un turno el {inicioConflicto:dd/MM/yyyy} " +
                    $"de {inicioConflicto:HH:mm} a {finConflicto:HH:mm}."
                );
            }

            var conflictoPaciente =
    dto.EstadoTurno != EnumEstadoTurno.cancelado
        ? await context.Turnos
            .Where(otroTurno =>
                otroTurno.Id != id &&
                otroTurno.EstadoRegistro ==
                    EnumEstadoRegistro.activo &&
                otroTurno.EstadoTurno !=
                    EnumEstadoTurno.cancelado &&
                otroTurno.FechaInicio < fechaFinUtc &&
                otroTurno.FechaFin > fechaInicioUtc &&
                otroTurno.TurnoPacientes.Any(
                    relacion =>
                        pacienteIds.Contains(
                            relacion.PacienteId
                        )
                )
            )
            .Select(otroTurno => new
            {
                otroTurno.FechaInicio,
                otroTurno.FechaFin,

                NombrePaciente =
                    otroTurno.TurnoPacientes
                        .Where(relacion =>
                            pacienteIds.Contains(
                                relacion.PacienteId
                            )
                        )
                        .Select(relacion =>
                            relacion.Pacientes != null
                                ? relacion.Pacientes.Nombre
                                : null
                        )
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync()
        : null;

            if (conflictoPaciente is not null)
            {
                var inicioConflicto =
                    conflictoPaciente.FechaInicio;

                var finConflicto =
                    conflictoPaciente.FechaFin;

                var nombrePaciente =
                    conflictoPaciente.NombrePaciente
                    ?? "Paciente seleccionado";

                throw new ApplicationException(
                    $"El paciente \"{nombrePaciente}\" " +
                    $"ya tiene un turno el {inicioConflicto:dd/MM/yyyy} " +
                    $"de {inicioConflicto:HH:mm} a {finConflicto:HH:mm}."
                );
            }


            turno.FechaInicio = fechaInicioUtc;
            turno.FechaFin = fechaFinUtc;
            turno.EstadoTurno = dto.EstadoTurno;
            turno.TipoTurnoId = tipoTurnoId;
            turno.TipoConsultorioId = dto.TipoConsultorioId;

            var obrasSocialesGuardadas =
    turno.TurnoPacientes
        .ToDictionary(
            relacion => relacion.PacienteId,
            relacion => new
            {
                relacion.TipoObraSocialId,
                relacion.NombreObraSocial
            }
        );

            var datosPacientesActuales =
                await context.Pacientes
                    .Where(paciente =>
                        pacienteIds.Contains(paciente.Id)
                    )
                    .Select(paciente => new
                    {
                        paciente.Id,
                        paciente.TipoObraSocialId,

                        NombreObraSocial =
                            paciente.TipoObraSociales != null
                                ? paciente.TipoObraSociales.Tipo
                                : null
                    })
                    .ToDictionaryAsync(
                        paciente => paciente.Id
                    );

            if (datosPacientesActuales.Count != pacienteIds.Count)
            {
                throw new ApplicationException(
                    "Uno o más pacientes seleccionados no existen."
                );
            }

            context.RemoveRange(turno.TurnoProfesionales);

            context.RemoveRange(turno.TurnoPacientes);

            context.RemoveRange(
                turno.TurnoTipoPrestaciones
            );

            var nuevosTurnoProfesionales = profesionalIds.Select(profesionalId => new TurnoProfesional
            {
                TurnoId = turno.Id,
                ProfesionalId =
                                profesionalId
            }
                    ).ToList();

            var nuevosTurnoPacientes =
    pacienteIds
        .Select(pacienteId =>
        {
            var datosActuales =
                datosPacientesActuales[pacienteId];

            var teniaObraSocialGuardada =
                obrasSocialesGuardadas.TryGetValue(
                    pacienteId,
                    out var datosGuardados
                ) &&
                (
                    datosGuardados.TipoObraSocialId.HasValue ||
                    !string.IsNullOrWhiteSpace(
                        datosGuardados.NombreObraSocial
                    )
                );

            return new TurnoPaciente
            {
                TurnoId =
                    turno.Id,

                PacienteId =
                    pacienteId,

                TipoObraSocialId =
                    teniaObraSocialGuardada
                        ? datosGuardados!.TipoObraSocialId
                        : datosActuales.TipoObraSocialId,

                NombreObraSocial =
                    teniaObraSocialGuardada
                        ? datosGuardados!.NombreObraSocial
                        : datosActuales.NombreObraSocial,

                EstadoRegistro =
                    EnumEstadoRegistro.activo
            };
        })
        .ToList();

            var nuevasTurnoTipoPrestaciones =
                profesionalPrestaciones
                    .Select(seleccion =>
                        new TurnoTipoPrestacion
                        {
                            TurnoId = turno.Id,
                            ProfesionalId = seleccion.ProfesionalId,
                            TipoPrestacionId = seleccion.TipoPrestacionId,
                            EstadoRegistro = EnumEstadoRegistro.activo
                        }
                    )
                    .ToList();

            context.AddRange(
                nuevosTurnoProfesionales
            );

            context.AddRange(
                nuevosTurnoPacientes
            );

            context.AddRange(
                nuevasTurnoTipoPrestaciones
            );

            await context.SaveChangesAsync();

            return true;
        }

        private IQueryable<Turno> ConsultarTurnosQueBloquean(int consultorioId, List<int> profesionalIds, List<int> pacienteIds)
        {
            return context.Turnos
                .Where(turno =>
                    turno.EstadoRegistro ==
                        EnumEstadoRegistro.activo &&

                    turno.EstadoTurno !=
                        EnumEstadoTurno.cancelado &&

                    (
                        turno.TipoConsultorioId ==
                            consultorioId ||

                        turno.TurnoProfesionales.Any(
                            relacion =>
                                profesionalIds.Contains(
                                    relacion.ProfesionalId
                                )
                        ) ||

                        turno.TurnoPacientes.Any(
                            relacion =>
                                pacienteIds.Contains(
                                    relacion.PacienteId
                                )
                        )
                    )
                );
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
            List<TurnoProfesionalPrestacionDTO> profesionalPrestaciones,
            int tipoTurnoId,
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

            var inicioHoyUtc =
                DateTime.SpecifyKind(
                    DateTime.UtcNow.Date,
                    DateTimeKind.Utc
                );

            var turnosSerie =
                await context.Turnos
                    .Include(turno =>
                        turno.TurnoProfesionales
                    )
                    .Include(turno =>
                        turno.TurnoPacientes
                    )
                    .Include(turno =>
                        turno.TurnoTipoPrestaciones
                    )
                    .Where(turno =>
                        turno.SerieTurnoId == serieTurnoId &&
                        turno.EstadoRegistro ==
                            EnumEstadoRegistro.activo &&
                        turno.FechaInicio >= inicioHoyUtc
                    )
                    .OrderBy(turno =>
                        turno.FechaInicio
                    )
                    .ToListAsync();

            if (turnosSerie.Count == 0)
            {
                throw new ApplicationException(
                    "No se encontraron turnos futuros en la serie."
                );
            }

            var turnoIds =
                turnosSerie
                    .Select(turno =>
                        turno.Id
                    )
                    .ToList();

            // Conservamos la fecha original de cada turno
            // y aplicamos la nueva hora.
            var nuevasFechasInicio =
                turnosSerie
                    .Select(turno =>
                        DateTime.SpecifyKind(
                            DateOnly
                                .FromDateTime(turno.FechaInicio)
                                .ToDateTime(dto.Hora),
                            DateTimeKind.Utc
                        )
                    )
                    .ToList();

            // Una serie cancelada deja de reservar recursos.
            if (dto.EstadoTurno !=
                EnumEstadoTurno.cancelado)
            {
                var primeraFechaInicio =
                    nuevasFechasInicio.Min();

                var ultimaFechaFin =
                    nuevasFechasInicio
                        .Max()
                        .AddMinutes(duracionMinutos);

                // Conflictos de consultorio.
                var turnosDelConsultorio =
                    await context.Turnos
                        .Where(turno =>
                            !turnoIds.Contains(turno.Id) &&
                            turno.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            turno.EstadoTurno !=
                                EnumEstadoTurno.cancelado &&
                            turno.TipoConsultorioId ==
                                dto.TipoConsultorioId &&
                            turno.FechaInicio <
                                ultimaFechaFin &&
                            turno.FechaFin >
                                primeraFechaInicio
                        )
                        .Select(turno => new
                        {
                            turno.FechaInicio,
                            turno.FechaFin,

                            NombreConsultorio =
                                turno.TipoConsultorios != null
                                    ? turno.TipoConsultorios.Tipo
                                    : "Consultorio"
                        })
                        .ToListAsync();

                var conflictosConsultorio =
                    turnosDelConsultorio
                        .Where(turnoExistente =>
                            nuevasFechasInicio.Any(
                                nuevaFechaInicio =>
                                    turnoExistente.FechaInicio <
                                        nuevaFechaInicio.AddMinutes(
                                            duracionMinutos
                                        ) &&
                                    turnoExistente.FechaFin >
                                        nuevaFechaInicio
                            )
                        )
                        .OrderBy(conflicto =>
                            conflicto.FechaInicio
                        )
                        .ToList();

                if (conflictosConsultorio.Count > 0)
                {
                    var nombreConsultorio =
                        conflictosConsultorio[0]
                            .NombreConsultorio;

                    var horariosTexto =
                        string.Join(
                            ", ",
                            conflictosConsultorio.Select(
                                conflicto =>
                                    $"{conflicto.FechaInicio:dd/MM/yyyy} " +
                                    $"de {conflicto.FechaInicio:HH:mm} " +
                                    $"a {conflicto.FechaFin:HH:mm}"
                            )
                        );

                    throw new ApplicationException(
                        $"El consultorio \"{nombreConsultorio}\" " +
                        "impide actualizar los turnos futuros de la serie " +
                        "porque ya está ocupado en estos horarios: " +
                        $"{horariosTexto}."
                    );
                }

                // Conflictos de profesionales.
                var turnosDeProfesionales =
                    await context.Turnos
                        .Where(turno =>
                            !turnoIds.Contains(turno.Id) &&
                            turno.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            turno.EstadoTurno !=
                                EnumEstadoTurno.cancelado &&
                            turno.FechaInicio <
                                ultimaFechaFin &&
                            turno.FechaFin >
                                primeraFechaInicio
                        )
                        .SelectMany(
                            turno =>
                                turno.TurnoProfesionales
                                    .Where(relacion =>
                                        profesionalIds.Contains(
                                            relacion.ProfesionalId
                                        )
                                    ),
                            (turno, relacion) => new
                            {
                                turno.FechaInicio,
                                turno.FechaFin,

                                NombreProfesional =
                                    relacion.Profesionales != null
                                        ? relacion.Profesionales.Nombre
                                        : "Profesional"
                            }
                        )
                        .ToListAsync();

                var conflictosProfesionales =
                    turnosDeProfesionales
                        .Where(turnoExistente =>
                            nuevasFechasInicio.Any(
                                nuevaFechaInicio =>
                                    turnoExistente.FechaInicio <
                                        nuevaFechaInicio.AddMinutes(
                                            duracionMinutos
                                        ) &&
                                    turnoExistente.FechaFin >
                                        nuevaFechaInicio
                            )
                        )
                        .OrderBy(conflicto =>
                            conflicto.FechaInicio
                        )
                        .ToList();

                if (conflictosProfesionales.Count > 0)
                {
                    var conflictosTexto =
                        string.Join(
                            ", ",
                            conflictosProfesionales.Select(
                                conflicto =>
                                    $"\"{conflicto.NombreProfesional}\" " +
                                    $"el {conflicto.FechaInicio:dd/MM/yyyy} " +
                                    $"de {conflicto.FechaInicio:HH:mm} " +
                                    $"a {conflicto.FechaFin:HH:mm}"
                            )
                        );

                    var inicioMensaje =
                        conflictosProfesionales.Count == 1
                            ? "El profesional"
                            : "Los profesionales";

                    var verbo =
                        conflictosProfesionales.Count == 1
                            ? "ya tiene"
                            : "ya tienen";

                    throw new ApplicationException(
                        $"{inicioMensaje} {conflictosTexto} " +
                        $"{verbo} un turno y no permite actualizar " +
                        "los turnos futuros de la serie."
                    );
                }

                // Conflictos de pacientes.
                var turnosDePacientes =
                    await context.Turnos
                        .Where(turno =>
                            !turnoIds.Contains(turno.Id) &&
                            turno.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            turno.EstadoTurno !=
                                EnumEstadoTurno.cancelado &&
                            turno.FechaInicio <
                                ultimaFechaFin &&
                            turno.FechaFin >
                                primeraFechaInicio
                        )
                        .SelectMany(
                            turno =>
                                turno.TurnoPacientes
                                    .Where(relacion =>
                                        pacienteIds.Contains(
                                            relacion.PacienteId
                                        )
                                    ),
                            (turno, relacion) => new
                            {
                                turno.FechaInicio,
                                turno.FechaFin,

                                NombrePaciente =
                                    relacion.Pacientes != null
                                        ? relacion.Pacientes.Nombre
                                        : "Paciente"
                            }
                        )
                        .ToListAsync();

                var conflictosPacientes =
                    turnosDePacientes
                        .Where(turnoExistente =>
                            nuevasFechasInicio.Any(
                                nuevaFechaInicio =>
                                    turnoExistente.FechaInicio <
                                        nuevaFechaInicio.AddMinutes(
                                            duracionMinutos
                                        ) &&
                                    turnoExistente.FechaFin >
                                        nuevaFechaInicio
                            )
                        )
                        .OrderBy(conflicto =>
                            conflicto.FechaInicio
                        )
                        .ToList();

                if (conflictosPacientes.Count > 0)
                {
                    var conflictosTexto =
                        string.Join(
                            ", ",
                            conflictosPacientes.Select(
                                conflicto =>
                                    $"\"{conflicto.NombrePaciente}\" " +
                                    $"el {conflicto.FechaInicio:dd/MM/yyyy} " +
                                    $"de {conflicto.FechaInicio:HH:mm} " +
                                    $"a {conflicto.FechaFin:HH:mm}"
                            )
                        );

                    var inicioMensaje =
                        conflictosPacientes.Count == 1
                            ? "El paciente"
                            : "Los pacientes";

                    var verbo =
                        conflictosPacientes.Count == 1
                            ? "ya tiene"
                            : "ya tienen";

                    throw new ApplicationException(
                        $"{inicioMensaje} {conflictosTexto} " +
                        $"{verbo} un turno y no permite actualizar " +
                        "los turnos futuros de la serie."
                    );
                }
            }

            var obrasSocialesGuardadas =
    turnosSerie
        .SelectMany(turno =>
            turno.TurnoPacientes.Select(
                relacion => new
                {
                    TurnoId =
                        turno.Id,

                    relacion.PacienteId,
                    relacion.TipoObraSocialId,
                    relacion.NombreObraSocial
                }
            )
        )
        .ToDictionary(
            item => (
                item.TurnoId,
                item.PacienteId
            ),
            item => new
            {
                item.TipoObraSocialId,
                item.NombreObraSocial
            }
        );

            var datosPacientesActuales =
                await context.Pacientes
                    .Where(paciente =>
                        pacienteIds.Contains(paciente.Id)
                    )
                    .Select(paciente => new
                    {
                        paciente.Id,
                        paciente.TipoObraSocialId,

                        NombreObraSocial =
                            paciente.TipoObraSociales != null
                                ? paciente.TipoObraSociales.Tipo
                                : null
                    })
                    .ToDictionaryAsync(
                        paciente => paciente.Id
                    );

            if (datosPacientesActuales.Count != pacienteIds.Count)
            {
                throw new ApplicationException(
                    "Uno o más pacientes seleccionados no existen."
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
                            fechaTurno.ToDateTime(
                                dto.Hora
                            ),
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

                    context.RemoveRange(
                        turno.TurnoTipoPrestaciones
                    );
                }

                // Guardamos la eliminación de las relaciones anteriores.
                await context.SaveChangesAsync();

                var nuevasRelacionesProfesionales =
                    turnosSerie
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

                var nuevasRelacionesPacientes =
    turnosSerie
        .SelectMany(turno =>
            pacienteIds.Select(
                pacienteId =>
                {
                    var datosActuales =
                        datosPacientesActuales[
                            pacienteId
                        ];

                    var teniaObraSocialGuardada =
                        obrasSocialesGuardadas
                            .TryGetValue(
                                (
                                    turno.Id,
                                    pacienteId
                                ),
                                out var datosGuardados
                            ) &&
                        (
                            datosGuardados
                                .TipoObraSocialId
                                .HasValue ||
                            !string.IsNullOrWhiteSpace(
                                datosGuardados
                                    .NombreObraSocial
                            )
                        );

                    return new TurnoPaciente
                    {
                        TurnoId =
                            turno.Id,

                        PacienteId =
                            pacienteId,

                        TipoObraSocialId =
                            teniaObraSocialGuardada
                                ? datosGuardados!
                                    .TipoObraSocialId
                                : datosActuales
                                    .TipoObraSocialId,

                        NombreObraSocial =
                            teniaObraSocialGuardada
                                ? datosGuardados!
                                    .NombreObraSocial
                                : datosActuales
                                    .NombreObraSocial,

                        EstadoRegistro =
                            EnumEstadoRegistro.activo
                    };
                }
            )
        )
        .ToList();

                var nuevasRelacionesPrestaciones =
                    turnosSerie
                        .SelectMany(turno =>
                            profesionalPrestaciones.Select(
                                seleccion =>
                                    new TurnoTipoPrestacion
                                    {
                                        TurnoId = turno.Id,
                                        ProfesionalId = seleccion.ProfesionalId,
                                        TipoPrestacionId = seleccion.TipoPrestacionId,
                                        EstadoRegistro = EnumEstadoRegistro.activo
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

                context.AddRange(
                    nuevasRelacionesPrestaciones
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

using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using CentroSenderos_2026_Shared.Recurrencias;



namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TurnoRepositorio : Repositorio<Turno>, ITurnoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TurnoRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TurnoDTO?> SelectPorId(int id)
        {
            return await context.Turnos
                .Include(t => t.TurnoProfesionales)
                    .ThenInclude(tp => tp.Profesionales)
                .Include(t => t.TurnoPacientes)
                    .ThenInclude(tp => tp.Pacientes)
                .Where(t => t.Id == id)
                .Select(t => new TurnoDTO
                {
                    Id = t.Id,
                    Fecha = t.FechaInicio.ToLocalTime().Date,
                    Hora = TimeOnly.FromDateTime(
                        t.FechaInicio.ToLocalTime()
                    ),
                    FechaFin = t.FechaFin.ToLocalTime(),
                    EstadoTurno = t.EstadoTurno,
                    TipoTurnoId = t.TipoTurnoId ?? -1,
                    TipoConsultorioId = t.TipoConsultorioId,

                    DuracionPersonalizada = t.TipoTurnoId == null
                        ? (int)(t.FechaFin - t.FechaInicio).TotalMinutes
                        : 0,

                    // Propiedades anteriores.
                    // Se mantienen mientras migramos el frontend.
                    ProfesionalId = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .FirstOrDefault(),

                    NombreProfesional = t.TurnoProfesionales
                        .Select(tp => tp.Profesionales!.Nombre)
                        .FirstOrDefault(),

                    PacienteId = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
                        .FirstOrDefault(),

                    NombrePaciente = t.TurnoPacientes
                        .Select(tp => tp.Pacientes!.Nombre)
                        .FirstOrDefault(),

                    // Nuevas listas con todas las relaciones.
                    ProfesionalIds = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .ToList(),

                    PacienteIds = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
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
                .Where(t =>
                    t.EstadoRegistro == EnumEstadoRegistro.activo
                )
                .Select(t => new TurnoListadoDTO
                {
                    Id = t.Id,
                    FechaInicio = t.FechaInicio.ToLocalTime(),
                    FechaFin = t.FechaFin.ToLocalTime(),
                    EstadoTurno = t.EstadoTurno,

                    TipoTurnoId = t.TipoTurnoId ?? -1,

                    NombreTipoTurno = t.TipoTurnos != null
                        ? t.TipoTurnos.Tipo
                        : $"Otro ({(int)(t.FechaFin - t.FechaInicio).TotalMinutes} min)",

                    TipoConsultorioId = t.TipoConsultorioId,

                    NombreTipoConsultorio = t.TipoConsultorios != null
                        ? t.TipoConsultorios.Tipo
                        : string.Empty,

                    SerieTurnoId = t.SerieTurnoId,

                    FrecuenciaRecurrencia = t.SerieTurno != null
                    ? t.SerieTurno.Frecuencia
                    : EnumFrecuenciaRecurrenciaTurno.noRepite,

                    IntervaloRecurrencia = t.SerieTurno != null
                    ? t.SerieTurno.Intervalo
                    : 1,

                    UnidadRecurrencia = t.SerieTurno != null
                    ? t.SerieTurno.UnidadPersonalizada
                    : null,

                    FechaHastaRecurrencia = t.SerieTurno != null
                    ? t.SerieTurno.FechaHasta.ToLocalTime()
                    : null,


                    // Propiedades anteriores.
                    // Se mantienen mientras migramos el frontend.
                    ProfesionalId = t.TurnoProfesionales
                        .Select(tp => tp.ProfesionalId)
                        .FirstOrDefault(),

                    NombreProfesional = t.TurnoProfesionales
                        .Select(tp => tp.Profesionales!.Nombre)
                        .FirstOrDefault(),

                    PacienteId = t.TurnoPacientes
                        .Select(tp => tp.PacienteId)
                        .FirstOrDefault(),

                    NombrePaciente = t.TurnoPacientes
                        .Select(tp => tp.Pacientes!.Nombre)
                        .FirstOrDefault(),

                    // Todos los profesionales asociados.
                    ProfesionalIds = t.TurnoProfesionales
                        .OrderBy(tp => tp.Profesionales!.Nombre)
                        .Select(tp => tp.ProfesionalId)
                        .ToList(),

                    NombresProfesionales = t.TurnoProfesionales
                        .OrderBy(tp => tp.Profesionales!.Nombre)
                        .Select(tp => tp.Profesionales!.Nombre)
                        .ToList(),

                    // Todos los pacientes asociados.
                    PacienteIds = t.TurnoPacientes
                        .OrderBy(tp => tp.Pacientes!.Nombre)
                        .Select(tp => tp.PacienteId)
                        .ToList(),

                    NombresPacientes = t.TurnoPacientes
                        .OrderBy(tp => tp.Pacientes!.Nombre)
                        .Select(tp => tp.Pacientes!.Nombre)
                        .ToList()
                })
                .ToListAsync();
        }


        public async Task<int> InsertarTurno(TurnoDTO dto)
        {
            if (dto.Hora == TimeOnly.MinValue)
                throw new ApplicationException("Debe seleccionar una hora válida.");
            if (dto.TipoTurnoId == 0)
                throw new ApplicationException("Debe seleccionar un tipo de turno válido o 'Otro'.");
            if (dto.TipoConsultorioId <= 0)
                throw new ApplicationException("Debe seleccionar un consultorio válido.");
            var profesionalIds = dto.ProfesionalIds
    .Where(id => id > 0)
    .Distinct()
    .ToList();

            var pacienteIds = dto.PacienteIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // Compatibilidad temporal con el frontend anterior.
            if (profesionalIds.Count == 0 && dto.ProfesionalId > 0)
            {
                profesionalIds.Add(dto.ProfesionalId);
            }

            if (pacienteIds.Count == 0 && dto.PacienteId > 0)
            {
                pacienteIds.Add(dto.PacienteId);
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
                CalculadorRecurrenciaTurno.CalcularFechas(
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

            var fechasConConflicto = await context.Turnos
                .Where(turno =>
                    turno.EstadoRegistro == EnumEstadoRegistro.activo
                    && turno.TipoConsultorioId == dto.TipoConsultorioId
                    && fechasInicioUtc.Contains(turno.FechaInicio)
                )
                .Select(turno => turno.FechaInicio)
                .OrderBy(fecha => fecha)
                .ToListAsync();

            if (fechasConConflicto.Count > 0)
            {
                var fechasTexto = string.Join(
                    ", ",
                    fechasConConflicto.Select(fecha =>
                        fecha.ToString("dd/MM/yyyy HH:mm")
                    )
                );

                throw new ApplicationException($"No se puede crear la serie porque ya existen turnos en estos horarios: {fechasTexto}.");
            }

            //var fechaInicioUtc = fechasInicioUtc.First();

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

                duracionMinutos = dto.DuracionPersonalizada;
            }
            else
            {
                var tipoTurno = await context.TipoTurnos
                    .FirstOrDefaultAsync(tipo =>
                        tipo.Id == tipoTurnoId
                    );

                if (tipoTurno is null)
                {
                    throw new ApplicationException(
                        $"No existe el tipo de turno con id {dto.TipoTurnoId}."
                    );
                }

                duracionMinutos = tipoTurno.DuracionMinutos;
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync();

            try
            {
                SerieTurno? serieTurno = null;

                var esRecurrente =
                    dto.FrecuenciaRecurrencia !=
                    EnumFrecuenciaRecurrenciaTurno.noRepite;

                if (esRecurrente)
                {
                    serieTurno = new SerieTurno
                    {
                        Frecuencia = dto.FrecuenciaRecurrencia,
                        Intervalo = dto.IntervaloRecurrencia,

                        UnidadPersonalizada =
                            dto.FrecuenciaRecurrencia ==
                                EnumFrecuenciaRecurrenciaTurno.personalizado
                                ? dto.UnidadRecurrencia
                                : null,

                        FechaInicio = DateTime.SpecifyKind(
                            dto.Fecha.Date,
                            DateTimeKind.Utc
                        ),

                        FechaHasta = DateTime.SpecifyKind(
                            dto.FechaHastaRecurrencia!.Value.Date,
                            DateTimeKind.Utc
                        ),

                        EstadoRegistro = EnumEstadoRegistro.activo
                    };

                    context.SeriesTurnos.Add(serieTurno);

                    await context.SaveChangesAsync();
                }

                var turnos = fechasInicioUtc
                    .Select(fechaInicio => new Turno
                    {
                        FechaInicio = fechaInicio,

                        FechaFin = fechaInicio.AddMinutes(
                            duracionMinutos
                        ),

                        EstadoTurno = dto.EstadoTurno,
                        TipoTurnoId = tipoTurnoId,
                        TipoConsultorioId = dto.TipoConsultorioId,

                        SerieTurnoId = serieTurno?.Id,

                        EstadoRegistro =
                            EnumEstadoRegistro.activo
                    })
                    .ToList();

                context.Turnos.AddRange(turnos);

                // Guardamos para que EF Core genere los Id de los turnos.
                await context.SaveChangesAsync();

                var relacionesProfesionales = turnos
                    .SelectMany(turno =>
                        profesionalIds.Select(profesionalId =>
                            new TurnoProfesional
                            {
                                TurnoId = turno.Id,
                                ProfesionalId = profesionalId
                            }
                        )
                    )
                    .ToList();

                var relacionesPacientes = turnos
                    .SelectMany(turno =>
                        pacienteIds.Select(pacienteId =>
                            new TurnoPaciente
                            {
                                TurnoId = turno.Id,
                                PacienteId = pacienteId
                            }
                        )
                    )
                    .ToList();

                context.AddRange(relacionesProfesionales);
                context.AddRange(relacionesPacientes);

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


        public async Task<List<TimeOnly>> HorariosDisponibles(DateOnly fecha, int tipoTurnoId, int consultorioId)
        {
            var tipoTurno = await context.TipoTurnos.FirstOrDefaultAsync(t => t.Id == tipoTurnoId);
            if (tipoTurno == null)
                throw new ApplicationException("Tipo de turno inválido.");

            var duracion = tipoTurno.DuracionMinutos;
            var inicioDia = new TimeOnly(8, 0);
            var finDia = new TimeOnly(20, 0);

            var horarios = new List<TimeOnly>();
            var horaActual = inicioDia;
            while (horaActual.AddMinutes(duracion) <= finDia)
            {
                var fechaHoraUtc = DateTime.SpecifyKind(fecha.ToDateTime(horaActual), DateTimeKind.Utc);

                var ocupado = await context.Turnos.AnyAsync(t =>
                    t.EstadoRegistro == EnumEstadoRegistro.activo &&
                    t.TipoConsultorioId == consultorioId &&
                    t.FechaInicio == fechaHoraUtc
                );

                if (!ocupado)
                    horarios.Add(horaActual);

                horaActual = horaActual.AddMinutes(duracion);
            }

            return horarios;
        }
        public async Task<bool> ActualizarTurno(int id, TurnoDTO dto)
        {
            if (dto.Hora == TimeOnly.MinValue)
                throw new ApplicationException("Debe seleccionar una hora válida.");
            if (dto.TipoTurnoId == 0)
                throw new ApplicationException("Debe seleccionar un tipo de turno válido o 'Otro'.");
            if (dto.TipoConsultorioId <= 0)
                throw new ApplicationException("Debe seleccionar un consultorio válido.");
            var profesionalIds = dto.ProfesionalIds
                .Where(profesionalId => profesionalId > 0)
                .Distinct()
                .ToList();

            var pacienteIds = dto.PacienteIds
                .Where(pacienteId => pacienteId > 0)
                .Distinct()
                .ToList();

            // Compatibilidad temporal con el frontend anterior.
            if (profesionalIds.Count == 0 && dto.ProfesionalId > 0)
            {
                profesionalIds.Add(dto.ProfesionalId);
            }

            if (pacienteIds.Count == 0 && dto.PacienteId > 0)
            {
                pacienteIds.Add(dto.PacienteId);
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
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null) return false;

            // Combinar fecha y hora en UTC
            DateOnly fecha = DateOnly.FromDateTime(dto.Fecha);
            DateTime fechaInicioUtc = DateTime.SpecifyKind(fecha.ToDateTime(dto.Hora), DateTimeKind.Utc);

            // 🔎 Validación: bloquear solo días anteriores
            if (fechaInicioUtc.Date < DateTime.UtcNow.Date)
                throw new ApplicationException("No se pueden mover turnos de días pasados.");

            // Validación de duplicados
            var existe = await context.Turnos.AnyAsync(t =>
                t.EstadoRegistro == EnumEstadoRegistro.activo &&
                t.TipoConsultorioId == dto.TipoConsultorioId &&
                t.FechaInicio == fechaInicioUtc &&
                t.Id != id
            );
            if (existe)
                throw new ApplicationException("Ya existe un turno en ese consultorio, fecha y hora.");

            // Calcular fecha fin
            DateTime fechaFinUtc;
            int? tipoTurnoId = dto.TipoTurnoId == -1 ? null : dto.TipoTurnoId;

            if (tipoTurnoId == null)
            {
                fechaFinUtc = fechaInicioUtc.AddMinutes(dto.DuracionPersonalizada);
            }
            else
            {
                var tipoTurno = await context.TipoTurnos.FirstOrDefaultAsync(t => t.Id == tipoTurnoId);
                if (tipoTurno == null)
                    throw new ApplicationException($"No existe el tipo de turno con id {dto.TipoTurnoId}");

                fechaFinUtc = fechaInicioUtc.AddMinutes(tipoTurno.DuracionMinutos);
            }

            // Actualizar datos del turno
            turno.FechaInicio = fechaInicioUtc;
            turno.FechaFin = fechaFinUtc;
            turno.EstadoTurno = dto.EstadoTurno;
            turno.TipoTurnoId = tipoTurnoId;
            turno.TipoConsultorioId = dto.TipoConsultorioId;

            // Eliminamos las relaciones anteriores.
            context.RemoveRange(turno.TurnoProfesionales);
            context.RemoveRange(turno.TurnoPacientes);

            // Creamos las nuevas relaciones con todos los profesionales.
            var nuevosTurnoProfesionales = profesionalIds
                .Select(profesionalId => new TurnoProfesional
                {
                    TurnoId = turno.Id,
                    ProfesionalId = profesionalId
                })
                .ToList();

            // Creamos las nuevas relaciones con todos los pacientes.
            var nuevosTurnoPacientes = pacienteIds
                .Select(pacienteId => new TurnoPaciente
                {
                    TurnoId = turno.Id,
                    PacienteId = pacienteId
                })
                .ToList();

            context.AddRange(nuevosTurnoProfesionales);
            context.AddRange(nuevosTurnoPacientes);

            await context.SaveChangesAsync();
            return true;
        }





        public async Task<bool> DeleteTurno(int id)
        {
            var turno = await context.Turnos.FirstOrDefaultAsync(t => t.Id == id);
            if (turno == null) return false;

            turno.EstadoRegistro = EnumEstadoRegistro.inactivo;
            await context.SaveChangesAsync();
            return true;
        }
    }
}


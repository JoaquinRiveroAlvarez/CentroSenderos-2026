using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

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
                .Where(t => t.Id == id)
                .Select(t => new TurnoDTO
                {
                    Id = t.Id,
                    // Ahora Fecha es DateTime en el DTO
                    Fecha = t.FechaInicio.ToLocalTime().Date, // devuelve DateTime con solo la fecha
                    Hora = TimeOnly.FromDateTime(t.FechaInicio.ToLocalTime()),
                    FechaFin = t.FechaFin.ToLocalTime(),
                    EstadoTurno = t.EstadoTurno,
                    TipoTurnoId = t.TipoTurnoId ?? -1,
                    TipoConsultorioId = t.TipoConsultorioId,
                    DuracionPersonalizada = t.TipoTurnoId == null
                        ? (int)(t.FechaFin - t.FechaInicio).TotalMinutes
                        : 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TurnoListadoDTO>> SelectListaTurnos()
        {
            return await context.Turnos
                .Include(t => t.TipoTurnos)
                .Include(t => t.TipoConsultorios)
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
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
                    NombreTipoConsultorio = t.TipoConsultorios != null ? t.TipoConsultorios.Tipo : ""
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

            // Convertir DateTime del front a DateOnly y luego a UTC
            DateOnly fecha = DateOnly.FromDateTime(dto.Fecha);
            DateTime fechaInicioUtc = DateTime.SpecifyKind(fecha.ToDateTime(dto.Hora), DateTimeKind.Utc);

            //if (fechaInicioUtc < DateTime.UtcNow)
            //    throw new ApplicationException("No se pueden cargar turnos en el pasado.");

            var existe = await context.Turnos.AnyAsync(t =>
                t.EstadoRegistro == EnumEstadoRegistro.activo &&
                t.TipoConsultorioId == dto.TipoConsultorioId &&
                t.FechaInicio == fechaInicioUtc
            );
            if (existe)
                throw new ApplicationException("Ya existe un turno en ese consultorio, fecha y hora.");

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

            var turno = new Turno
            {
                FechaInicio = fechaInicioUtc,
                FechaFin = fechaFinUtc,
                EstadoTurno = dto.EstadoTurno,
                TipoTurnoId = tipoTurnoId,
                TipoConsultorioId = dto.TipoConsultorioId,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Turnos.Add(turno);
            await context.SaveChangesAsync();
            return turno.Id;
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

            var turno = await context.Turnos.FirstOrDefaultAsync(t => t.Id == id);
            if (turno == null) return false;

            DateOnly fecha = DateOnly.FromDateTime(dto.Fecha);
            DateTime fechaInicioUtc = DateTime.SpecifyKind(fecha.ToDateTime(dto.Hora), DateTimeKind.Utc);

            if (fechaInicioUtc < DateTime.UtcNow)
                throw new ApplicationException("No se pueden mover turnos a fechas pasadas.");

            var existe = await context.Turnos.AnyAsync(t =>
                t.EstadoRegistro == EnumEstadoRegistro.activo &&
                t.TipoConsultorioId == dto.TipoConsultorioId &&
                t.FechaInicio == fechaInicioUtc &&
                t.Id != id
            );
            if (existe)
                throw new ApplicationException("Ya existe un turno en ese consultorio, fecha y hora.");

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

            turno.FechaInicio = fechaInicioUtc;
            turno.FechaFin = fechaFinUtc;
            turno.EstadoTurno = dto.EstadoTurno;
            turno.TipoTurnoId = tipoTurnoId;
            turno.TipoConsultorioId = dto.TipoConsultorioId;

            context.Turnos.Update(turno);
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


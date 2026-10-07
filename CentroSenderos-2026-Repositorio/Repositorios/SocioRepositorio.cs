using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class SocioRepositorio : Repositorio<Socio>, ISocioRepositorio
    {
        private readonly ApplicationDbContext context;

        public SocioRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<SocioListadoDTO?> SelectPorId(int id)
        {
            return await context.Socios
                .Where(s => s.Id == id)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    EsCaja = s.EsCaja,
                    Profesional = s.EsCaja
                        ? "Caja Senderos"
                        : s.Profesionales != null
                            ? s.Profesionales.Nombre
                            : "Socio",
                    Observacion = s.Observacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SocioListadoDTO?> SelectByProfesionalId(int profesionalId)
        {
            return await context.Socios
                .Where(s =>
                    !s.EsCaja &&
                    s.ProfesionalId == profesionalId)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    EsCaja = s.EsCaja,
                    Profesional = s.Profesionales != null
                        ? s.Profesionales.Nombre
                        : "Socio",
                    Observacion = s.Observacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SocioListadoDTO>> SelectListaSocios()
        {
            return await context.Socios
                .Where(s =>
                    s.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(s => s.EsCaja
                    ? "Caja Senderos"
                    : s.Profesionales != null
                        ? s.Profesionales.Nombre
                        : "Socio")
                .ThenBy(s => s.Id)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    EsCaja = s.EsCaja,
                    Profesional = s.EsCaja
                        ? "Caja Senderos"
                        : s.Profesionales != null
                            ? s.Profesionales.Nombre
                            : "Socio",
                    Observacion = s.Observacion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarSocio(SocioDTO dto)
        {
            if (dto == null)
            {
                throw new ApplicationException(
                    "No se recibieron los datos del socio.");
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            await ValidarDatosSocio(dto);

            var socio = new Socio
            {
                ProfesionalId = dto.ProfesionalId,
                EsCaja = dto.EsCaja,
                Observacion = dto.Observacion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Socios.Add(socio);

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return socio.Id;
        }

        public async Task<bool> DeleteSocio(int id)
        {
            var socio = await context.Socios
                .FirstOrDefaultAsync(s => s.Id == id);

            if (socio == null)
                return false;

            if (socio.EsCaja)
            {
                throw new ApplicationException(
                    "No se puede dar de baja Caja Senderos.");
            }

            socio.EstadoRegistro = EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActualizarSocio(int id, SocioDTO dto)
        {
            if (dto == null)
            {
                throw new ApplicationException(
                    "No se recibieron los datos del socio.");
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var socio = await context.Socios
                .FirstOrDefaultAsync(s => s.Id == id);

            if (socio == null)
                return false;

            if (socio.EsCaja != dto.EsCaja)
            {
                throw new ApplicationException(
                    "No se puede convertir un socio en Caja Senderos " +
                    "ni convertir Caja Senderos en un socio.");
            }

            if (socio.EsCaja && dto.ProfesionalId != null)
            {
                throw new ApplicationException(
                    "Caja Senderos no puede tener un profesional asociado.");
            }

            if (socio.ProfesionalId != dto.ProfesionalId)
            {
                var tienePagos = await context.Gastos
                    .AnyAsync(gasto =>
                        gasto.GastoSocios.Any(aporte =>
                            aporte.SocioId == id));

                var tieneRepartos = await context.GastoRepartos
                    .AnyAsync(reparto =>
                        reparto.SocioId == id);

                var tieneReintegros = await context.ReintegrosSocios
                    .AnyAsync(reintegro =>
                        reintegro.SocioPagadorId == id ||
                        reintegro.SocioReceptorId == id);

                if (tienePagos || tieneRepartos || tieneReintegros)
                {
                    throw new ApplicationException(
                        "No se puede cambiar el profesional de un socio " +
                        "que tiene pagos, repartos o reintegros registrados. " +
                        "Podés modificar su observación.");
                }

                await ValidarDatosSocio(dto, id);

                socio.ProfesionalId = dto.ProfesionalId;
            }

            socio.Observacion = dto.Observacion;

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return true;
        }

        public async Task<List<SocioListadoDTO>> SelectListaSociosParaReparto()
        {
            return await context.Socios
                .AsNoTracking()
                .OrderBy(s => s.EsCaja
                    ? "Caja Senderos"
                    : s.Profesionales != null
                        ? s.Profesionales.Nombre
                        : "Socio")
                .ThenBy(s => s.Id)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    EsCaja = s.EsCaja,
                    Profesional = s.EsCaja
                        ? "Caja Senderos"
                        : s.Profesionales != null
                            ? s.Profesionales.Nombre
                            : "Socio",
                    Observacion = s.Observacion
                })
                .ToListAsync();
        }

        private async Task ValidarDatosSocio(SocioDTO dto,int? socioId = null)
        {
            if (dto.EsCaja)
            {
                if (dto.ProfesionalId != null)
                {
                    throw new ApplicationException(
                        "Caja Senderos no puede tener un profesional asociado.");
                }

                var cajaExiste = await context.Socios
                    .AnyAsync(s =>
                        s.EsCaja &&
                        (!socioId.HasValue || s.Id != socioId.Value));

                if (cajaExiste)
                {
                    throw new ApplicationException(
                        "Ya existe un registro de Caja Senderos.");
                }

                return;
            }

            if (!dto.ProfesionalId.HasValue ||
                dto.ProfesionalId.Value <= 0)
            {
                throw new ApplicationException(
                    "Seleccioná un profesional para el socio.");
            }

            var profesionalExiste = await context.Profesionales
                .AnyAsync(p =>
                    p.Id == dto.ProfesionalId.Value &&
                    p.EstadoRegistro == EnumEstadoRegistro.activo);

            if (!profesionalExiste)
            {
                throw new ApplicationException(
                    "El profesional seleccionado no existe o está inactivo.");
            }

            var profesionalYaEsSocio = await context.Socios
                .AnyAsync(s =>
                    s.ProfesionalId == dto.ProfesionalId &&
                    (!socioId.HasValue || s.Id != socioId.Value));

            if (profesionalYaEsSocio)
            {
                throw new ApplicationException(
                    "El profesional seleccionado ya tiene un registro de socio.");
            }
        }
    }
}
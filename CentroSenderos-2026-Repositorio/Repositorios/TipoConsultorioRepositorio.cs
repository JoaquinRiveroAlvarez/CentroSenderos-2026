using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoConsultorioRepositorio : Repositorio<TipoConsultorio>, ITipoConsultorioRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoConsultorioRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoConsultorioDTO?> SelectPorId(int id)
        {
            return await context.TipoConsultorios
                .Where(p => p.Id == id)
                .Select(p => new TipoConsultorioDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Direccion = p.Direccion,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoConsultorioListadoDTO?> SelectByTipoConsultorio(string tipo)
        {
            return await context.TipoConsultorios
                .Select(p => new TipoConsultorioListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Direccion = p.Direccion
                })
                .FirstOrDefaultAsync(x => x.Tipo == tipo);
        }

        public async Task<List<TipoConsultorioListadoDTO>> SelectListaTipoConsultorio()
        {
            return await context.TipoConsultorios
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => new TipoConsultorioListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Direccion = p.Direccion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTipoConsultorio(TipoConsultorioDTO dto)
        {
            var codLimpio = dto.Tipo.StartsWith("Tipo: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            var entidad = new TipoConsultorio
            {
                Tipo = codLimpio,
                Descripcion = dto.Descripcion,
                Direccion = dto.Direccion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoConsultorios.Add(entidad);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoConsultorio_Tipo_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un consultorio con el nombre '{codLimpio}'.");
                }
                throw;
            }

            return entidad.Id;
        }

        public async Task<bool> DeleteTipoConsultorio(int id)
        {
            var entidad = await context.TipoConsultorios.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoConsultorio(int id, TipoConsultorioDTO dto)
        {
            var entidad = await context.TipoConsultorios.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            var codLimpio = dto.Tipo.StartsWith("Tipo: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            var existe = await context.TipoConsultorios.AnyAsync(p => p.Tipo == codLimpio && p.Id != id);
            if (existe)
                throw new ApplicationException($"Ya existe un consultorio con el nombre '{codLimpio}'.");

            entidad.Tipo = codLimpio;
            entidad.Descripcion = dto.Descripcion;
            entidad.Direccion = dto.Direccion;

            try
            {
                context.TipoConsultorios.Update(entidad);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoConsultorio_Tipo_UQ") == true)
                    throw new ApplicationException($"Ya existe un consultorio con el nombre '{codLimpio}'.");
                throw;
            }
        }
    }
}


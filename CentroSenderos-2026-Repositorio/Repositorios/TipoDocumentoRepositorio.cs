using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Modelado2025_1Repositorio.Repositorios;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoDocumentoRepositorio : Repositorio<TipoDocumento>, ITipoDocumentoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoDocumentoRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoDocumentoDTO?> SelectPorId(int id)
        {
            return await context.TipoDocumentos
                .Where(p => p.Id == id)
                .Select(p => new TipoDocumentoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoDocumentoDTO?> SelectByTipoDocumento(string tipo)
        {
            return await context.TipoDocumentos
                .Where(p => p.Tipo == tipo)
                .Select(p => new TipoDocumentoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoDocumentoDTO>> SelectListaTipoDocumento()
        {
            return await context.TipoDocumentos
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(p => p.Tipo)
                .Select(p => new TipoDocumentoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();
        }



        public async Task<int> InsertarTipoDocumento(TipoDocumentoDTO dto)
        {
            var nombre = dto.Tipo?.Trim() ?? string.Empty;
            var descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(nombre) || nombre.Length > 50 || descripcion.Length > 50)
                throw new ApplicationException("Ingresá un nombre y una descripción de hasta 50 caracteres.");
            if (await context.TipoDocumentos.AnyAsync(t => t.Tipo.ToLower() == nombre.ToLower()))
                throw new ApplicationException("Ya existe un tipo de documento con ese nombre.");

            var entidad = new TipoDocumento
            {
                Tipo = nombre,
                Descripcion = descripcion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoDocumentos.Add(entidad);
            try { await context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("TipoDocumento_Tipo_UQ", StringComparison.OrdinalIgnoreCase) == true)
            { throw new ApplicationException("Ya existe un tipo de documento con ese nombre."); }

            return entidad.Id;
        }

        public async Task<bool> DeleteTipoDocumento(int id)
        {
            var entidad = await context.TipoDocumentos.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoDocumento(int id, TipoDocumentoDTO dto)
        {
            var entidad = await context.TipoDocumentos.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.Tipo = dto.Tipo;
            entidad.Descripcion = dto.Descripcion!;
            //entidad.EstadoRegistro = dto.EstadoRegistro;

            context.TipoDocumentos.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

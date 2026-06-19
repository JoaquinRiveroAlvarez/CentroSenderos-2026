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
    public class TipoObraSocialRepositorio : Repositorio<TipoObraSocial>, ITipoObraSocialRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoObraSocialRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<TipoListadoDTO?> SelectByTipoObraSocial(string tipo)
        {
            TipoListadoDTO? entidad = await context.TipoObrasSociales
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync(x => x.Tipo == tipo);
            return entidad;
        }
        public async Task<List<TipoListadoDTO>> SelectListaTipoObrasocial()
        {
            var lista = await context.TipoObrasSociales
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                })
                .ToListAsync();
            return lista;
        }
        public async Task<int> InsertarTipoObraSocial(TipoDTO dto)
        {
            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Tipo.StartsWith("Tipo: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            var tipoObraSocial = new TipoObraSocial
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                EstadoRegistro = EnumEstadoRegistro.EnGrabacion
            };

            context.TipoObrasSociales.Add(tipoObraSocial);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoObraSocial_Cod_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe una obra social con el nombre '{codlimpio}'.");
                }
                throw;
            }

            return tipoObraSocial.Id;
        }
        public async Task<bool> DeleteTipoObraSocial(int id)
        {
            var tipoObraSocial = await context.TipoObrasSociales
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoObraSocial == null)
                return false;

            // Finalmente eliminar el tipo de prestación
            context.TipoObrasSociales.Remove(tipoObraSocial);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoObraSocial(int id, TipoDTO dto)
        {
            var tipoObraSocial = await context.TipoObrasSociales
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoObraSocial == null)
                return false;

            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Tipo.StartsWith("Cod: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            // Verificar si el CUIT, MP o RNP ya existe (en otro registro)
            var cuitExiste = await context.TipoObrasSociales
                .AnyAsync(p => p.Tipo == codlimpio && p.Id != id);
            if (cuitExiste)
                throw new ApplicationException($"Ya existe una obra social con el nombre '{codlimpio}'.");

            // Actualizar los datos
            tipoObraSocial.Tipo = dto.Tipo;
            tipoObraSocial.Descripcion = dto.Descripcion;
            tipoObraSocial.EstadoRegistro = dto.EstadoRegistro;

            try
            {
                context.TipoObrasSociales.Update(tipoObraSocial);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoObrasSocial_Cod_UQ") == true)
                    throw new ApplicationException($"Ya existe una obra social con el nombre '{codlimpio}'.");
                throw;
            }
        }
    }
}

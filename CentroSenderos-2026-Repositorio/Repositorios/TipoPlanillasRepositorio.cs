using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoPlanillaRepositorio : Repositorio<TipoPlanilla>, ITipoPlanillaRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoPlanillaRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoDTO?> SelectPorId(int id)
        {
            return await context.TipoPlanillas
                .Where(p => p.Id == id)
                .Select(p => new TipoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoListadoDTO?> SelectByTipoPlanilla(string tipo)
        {
            return await context.TipoPlanillas
                .Where(p => p.Tipo == tipo)
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoListadoDTO>> SelectListaTipoPlanilla()
        {
            return await context.TipoPlanillas
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();
        }



        public async Task<int> InsertarTipoPlanilla(TipoDTO dto)
        {
            var entidad = new TipoPlanilla
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoPlanillas.Add(entidad);
            await context.SaveChangesAsync();

            return entidad.Id;
        }

        public async Task<bool> DeleteTipoPlanilla(int id)
        {
            var entidad = await context.TipoPlanillas.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoPlanilla(int id, TipoDTO dto)
        {
            var entidad = await context.TipoPlanillas.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.Tipo = dto.Tipo;
            entidad.Descripcion = dto.Descripcion;
            //entidad.EstadoRegistro = dto.EstadoRegistro;

            context.TipoPlanillas.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }
       }
}

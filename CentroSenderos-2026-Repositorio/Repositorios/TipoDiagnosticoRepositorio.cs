using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoDiagnosticoRepositorio : Repositorio<TipoDiagnostico>, ITipoDiagnosticoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoDiagnosticoRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoDTO?> SelectPorId(int id)
        {
            return await context.TipoDiagnosticos
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

        public async Task<TipoListadoDTO?> SelectByTipoDiagnostico(string tipo)
        {
            return await context.TipoDiagnosticos
                .Where(p => p.Tipo == tipo)
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoListadoDTO>> SelectListaTipoDiagnostico()
        {
            return await context.TipoDiagnosticos
                .Select(p => new TipoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();
        }



        public async Task<int> InsertarTipoDiagnostico(TipoDTO dto)
        {
            var entidad = new TipoDiagnostico
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                EstadoRegistro = EnumEstadoRegistro.EnGrabacion
            };

            context.TipoDiagnosticos.Add(entidad);
            await context.SaveChangesAsync();

            return entidad.Id;
        }

        public async Task<bool> DeleteTipoDiagnostico(int id)
        {
            var entidad = await context.TipoDiagnosticos.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            context.TipoDiagnosticos.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoDiagnostico(int id, TipoDTO dto)
        {
            var entidad = await context.TipoDiagnosticos.FirstOrDefaultAsync(p => p.Id == id);
            if (entidad == null) return false;

            entidad.Tipo = dto.Tipo;
            entidad.Descripcion = dto.Descripcion;
            entidad.EstadoRegistro = dto.EstadoRegistro;

            context.TipoDiagnosticos.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

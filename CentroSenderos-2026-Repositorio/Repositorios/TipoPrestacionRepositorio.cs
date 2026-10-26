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
    public class TipoPrestacionRepositorio : Repositorio<TipoPrestacion>, ITipoPrestacionRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoPrestacionRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoPrestacionDTO?> SelectPorId(int id)
        {
            return await context.TipoPrestaciones
                .Where(p => p.Id == id)
                .Select(p => new TipoPrestacionDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Cod = p.Cod,
                    MontoSesion = p.MontoSesion,
                    PorcentajeRetencion = p.PorcentajeRetencion,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoPrestacionListadoDTO?> SelectByCod(string cod)
        {
            return await context.TipoPrestaciones
                .Select(p => new TipoPrestacionListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Cod = p.Cod,
                    MontoSesion = p.MontoSesion,
                    PorcentajeRetencion = p.PorcentajeRetencion
                })
                .FirstOrDefaultAsync(x => x.Cod == cod);
        }

        public async Task<List<TipoPrestacionListadoDTO>> SelectListaTipoPrestacion()
        {
            return await context.TipoPrestaciones
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(p => p.Tipo)
                .Select(p => new TipoPrestacionListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Cod = p.Cod,
                    MontoSesion = p.MontoSesion,
                    PorcentajeRetencion = p.PorcentajeRetencion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTipoPrestacion(TipoPrestacionDTO dto)
        {
            var codlimpio = dto.Cod.StartsWith("Cod: ") ? dto.Cod.Substring(6) : dto.Cod;

            var tipoPrestacion = new TipoPrestacion
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Cod = codlimpio,
                MontoSesion = dto.MontoSesion,
                PorcentajeRetencion = dto.PorcentajeRetencion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoPrestaciones.Add(tipoPrestacion);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoPrestacion_Cod_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un tipo de prestación con el código '{codlimpio}'.");
                }
                throw;
            }

            return tipoPrestacion.Id;
        }

        public async Task<bool> DeleteTipoPrestacion(int id)
        {
            var tipoPrestacion = await context.TipoPrestaciones.FirstOrDefaultAsync(p => p.Id == id);
            if (tipoPrestacion == null) return false;

            tipoPrestacion.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoPrestacion(int id, TipoPrestacionDTO dto)
        {
            var tipoPrestacion = await context.TipoPrestaciones.FirstOrDefaultAsync(p => p.Id == id);
            if (tipoPrestacion == null) return false;

            var codlimpio = dto.Cod.StartsWith("Cod: ") ? dto.Cod.Substring(6) : dto.Cod;

            var cuitExiste = await context.TipoPrestaciones
                .AnyAsync(p => p.Cod == codlimpio && p.Id != id);

            if (cuitExiste)
                throw new ApplicationException($"Ya existe un tipo de prestación con el código '{codlimpio}'.");

            tipoPrestacion.Tipo = dto.Tipo;
            tipoPrestacion.Descripcion = dto.Descripcion;
            tipoPrestacion.Cod = codlimpio;
            tipoPrestacion.MontoSesion = dto.MontoSesion;
            tipoPrestacion.PorcentajeRetencion = dto.PorcentajeRetencion;

            try
            {
                context.TipoPrestaciones.Update(tipoPrestacion);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoPrestacion_Cod_UQ") == true)
                    throw new ApplicationException($"Ya existe un tipo de prestación con el código '{codlimpio}'.");
                throw;
            }
        }
    }
}
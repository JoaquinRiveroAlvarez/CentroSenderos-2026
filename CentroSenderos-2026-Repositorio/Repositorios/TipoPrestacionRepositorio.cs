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
    public class TipoPrestacionRepositorio : Repositorio<TipoPrestacion>, ITipoPrestacionRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoPrestacionRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<TipoPrestacionListadoDTO?> SelectByCod(string cod)
        {
            TipoPrestacionListadoDTO? entidad = await context.TipoPrestaciones
                .Select(p => new TipoPrestacionListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Cod = p.Cod,
                    MontoSesion = p.MontoSesion
                })
                .FirstOrDefaultAsync(x => x.Cod == cod);
            return entidad;
        }
        public async Task<List<TipoPrestacionListadoDTO>> SelectListaTipoPrestacion()
        {
            var lista = await context.TipoPrestaciones
                .Select(p => new TipoPrestacionListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    Cod = p.Cod,
                    MontoSesion = p.MontoSesion
                })
                .ToListAsync();
            return lista;
        }
        public async Task<int> InsertarTipoPrestacion(TipoPrestacionDTO dto)
        {
            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Cod.StartsWith("Cod: ") ? dto.Cod.Substring(6) : dto.Cod;

            var tipoPrestacion = new TipoPrestacion
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Cod = codlimpio,
                MontoSesion = dto.MontoSesion,
                EstadoRegistro = EnumEstadoRegistro.EnGrabacion
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
                    throw new ApplicationException($"Ya existe una tipo de prestación con el código '{codlimpio}'.");
                }
                throw;
            }

            return tipoPrestacion.Id;
        }
        public async Task<bool> DeleteTipoPrestacion(int id)
        {
            var tipoPrestacion = await context.TipoPrestaciones
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoPrestacion == null)
                return false;

            // Finalmente eliminar el tipo de prestación
            context.TipoPrestaciones.Remove(tipoPrestacion);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoPrestacion(int id, TipoPrestacionDTO dto)
        {
            var tipoPrestacion = await context.TipoPrestaciones
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoPrestacion == null)
                return false;

            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Cod.StartsWith("Cod: ") ? dto.Cod.Substring(6) : dto.Cod;

            // Verificar si el CUIT, MP o RNP ya existe (en otro registro)
            var cuitExiste = await context.TipoPrestaciones
                .AnyAsync(p => p.Cod == codlimpio && p.Id != id);
            if (cuitExiste)
                throw new ApplicationException($"Ya existe un tipo de prestación con el código '{codlimpio}'.");

            // Actualizar los datos
            tipoPrestacion.Tipo = dto.Tipo;
            tipoPrestacion.Descripcion = dto.Descripcion;
            tipoPrestacion.Cod = codlimpio;
            tipoPrestacion.MontoSesion = dto.MontoSesion;
            tipoPrestacion.EstadoRegistro = dto.EstadoRegistro;

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

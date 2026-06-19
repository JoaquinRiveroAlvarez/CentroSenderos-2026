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
    public class TipoTurnoRepositorio : Repositorio<TipoTurno>, ITipoTurnoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoTurnoRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<TipoTurnoListadoDTO?> SelectByTipoTurno(string tipo)
        {
            TipoTurnoListadoDTO? entidad = await context.TipoTurnos
                .Select(p => new TipoTurnoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    DuracionMinutos = p.DuracionMinutos
                })
                .FirstOrDefaultAsync(x => x.Tipo == tipo);
            return entidad;
        }
        public async Task<List<TipoTurnoListadoDTO>> SelectListaTipoTurno()
        {
            var lista = await context.TipoTurnos
                .Select(p => new TipoTurnoListadoDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo,
                    Descripcion = p.Descripcion,
                    DuracionMinutos = p.DuracionMinutos
                })
                .ToListAsync();
            return lista;
        }
        public async Task<int> InsertarTipoTurno(TipoTurnoDTO dto)
        {
            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Tipo.StartsWith("Tipo: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            var tipoTurno = new TipoTurno
            {
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                DuracionMinutos = dto.DuracionMinutos
            };

            context.TipoTurnos.Add(tipoTurno);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoTurno_Cod_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un tipo de turno con el nombre '{codlimpio}'.");
                }
                throw;
            }

            return tipoTurno.Id;
        }
        public async Task<bool> DeleteTipoTurno(int id)
        {
            var tipoTurno = await context.TipoTurnos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoTurno == null)
                return false;

            // Finalmente eliminar el tipo de prestación
            context.TipoTurnos.Remove(tipoTurno);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarTipoTurno(int id, TipoTurnoDTO dto)
        {
            var tipoTurno = await context.TipoTurnos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (tipoTurno == null)
                return false;

            // Validar que el CUIT no tenga prefijo duplicado
            var codlimpio = dto.Tipo.StartsWith("Cod: ") ? dto.Tipo.Substring(6) : dto.Tipo;

            // Verificar si el CUIT, MP o RNP ya existe (en otro registro)
            var cuitExiste = await context.TipoTurnos
                .AnyAsync(p => p.Tipo == codlimpio && p.Id != id);
            if (cuitExiste)
                throw new ApplicationException($"Ya existe un tipo de turno con el nombre '{codlimpio}'.");

            // Actualizar los datos
            tipoTurno.Tipo = dto.Tipo;
            tipoTurno.Descripcion = dto.Descripcion;
            tipoTurno.EstadoRegistro = dto.EstadoRegistro;

            try
            {
                context.TipoTurnos.Update(tipoTurno);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("TipoTurno_Cod_UQ") == true)
                    throw new ApplicationException($"Ya existe un tipo de turno con el nombre '{codlimpio}'.");
                throw;
            }
        }
    }
}

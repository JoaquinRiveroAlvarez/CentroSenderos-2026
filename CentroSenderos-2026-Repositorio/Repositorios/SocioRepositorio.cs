using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Modelado2025_1Repositorio.Repositorios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
                    Profesional = s.Profesionales!.Nombre,
                    Observacion = s.Observacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SocioListadoDTO?> SelectByProfesionalId(int profesionalId)
        {
            return await context.Socios
                .Where(s => s.ProfesionalId == profesionalId)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    Profesional = s.Profesionales!.Nombre,
                    Observacion = s.Observacion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SocioListadoDTO>> SelectListaSocios()
        {
            return await context.Socios
                .Where(s => s.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(s => s.Profesionales!.Nombre)
                .Select(s => new SocioListadoDTO
                {
                    Id = s.Id,
                    ProfesionalId = s.ProfesionalId,
                    Profesional = s.Profesionales!.Nombre,
                    Observacion = s.Observacion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarSocio(SocioDTO dto)
        {
            // Verificar si ya existe un socio para ese profesional
            var existe = await context.Socios
                .AnyAsync(s => s.ProfesionalId == dto.ProfesionalId);

            if (existe)
            {
                // Lanzamos una excepción controlada para que el controller la capture
                throw new ApplicationException(
                    $"El profesional ya es un socio {dto.ProfesionalId}."
                );
            }

            var socio = new Socio
            {
                ProfesionalId = dto.ProfesionalId,
                Observacion = dto.Observacion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Socios.Add(socio);
            await context.SaveChangesAsync();
            return socio.Id;
        }


        public async Task<bool> DeleteSocio(int id)
        {
            var socio = await context.Socios.FirstOrDefaultAsync(s => s.Id == id);
            if (socio == null) return false;

            socio.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarSocio(int id, SocioDTO dto)
        {
            var socio = await context.Socios.FirstOrDefaultAsync(s => s.Id == id);
            if (socio == null) return false;

            socio.ProfesionalId = dto.ProfesionalId;
            socio.Observacion = dto.Observacion;
            //socio.EstadoRegistro = dto.EstadoRegistro;

            context.Socios.Update(socio);
            await context.SaveChangesAsync();
            return true;
        }
    }
}


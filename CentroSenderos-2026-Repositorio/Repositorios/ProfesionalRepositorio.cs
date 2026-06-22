using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class ProfesionalRepositorio : Repositorio<Profesional>, IProfesionalRepositorio
    {
        private readonly ApplicationDbContext context;

        public ProfesionalRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<ProfesionalDTO?> SelectPorId(int id)
        {
            return await context.Profesionales
                .Where(p => p.Id == id)
                .Select(p => new ProfesionalDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Area = p.Area,
                    Cuit = p.Cuit,
                    MP = p.MP,
                    RNP = p.RNP,
                    Telefono = p.Telefono,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }
        public async Task<ProfesionalListadoDTO?> SelectByCuit(string cod)
        {
            ProfesionalListadoDTO? entidad = await context.Profesionales
                .Select(p => new ProfesionalListadoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Area = p.Area,
                    Cuit = p.Cuit,
                    MP = p.MP,
                    RNP = p.RNP,
                    Telefono = p.Telefono
                })
                .FirstOrDefaultAsync(x => x.Cuit == cod);
            return entidad;
        }
        public async Task<List<ProfesionalListadoDTO>> SelectListaProfesional()
        {
            var lista = await context.Profesionales
                .Select(p => new ProfesionalListadoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Area = p.Area,
                    Cuit = p.Cuit,
                    MP = p.MP,
                    RNP = p.RNP,
                    Telefono = p.Telefono
                })
                .ToListAsync();
            return lista;
        }
        public async Task<int> InsertarProfesional(ProfesionalDTO dto)
        {
            // Validar que el CUIT no tenga prefijo duplicado
            var cuitLimpio = dto.Cuit.StartsWith("CUIT: ") ? dto.Cuit.Substring(6) : dto.Cuit;

            var profesional = new Profesional
            {
                Nombre = dto.Nombre,
                Area = dto.Area,
                Cuit = cuitLimpio,
                MP = dto.MP,
                RNP = dto.RNP,
                Telefono = dto.Telefono,
                EstadoRegistro = EnumEstadoRegistro.EnGrabacion
            };

            context.Profesionales.Add(profesional);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Profesional_Cuit_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un profesional con el CUIT '{cuitLimpio}'.");
                }
                if (ex.InnerException?.Message.Contains("Profesional_MP_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un profesional con la Matrícula Profesional '{dto.MP}'.");
                }
                if (ex.InnerException?.Message.Contains("Profesional_RNP_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un profesional con el RNP '{dto.RNP}'.");
                }
                throw;
            }

            return profesional.Id;
        }
        public async Task<bool> DeleteProfesional(int id)
        {
            var profesional = await context.Profesionales
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesional == null)
                return false;

            // Finalmente eliminar el profesional
            context.Profesionales.Remove(profesional);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarProfesional(int id, ProfesionalDTO dto)
        {
            var profesional = await context.Profesionales
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesional == null)
                return false;

            // Validar que el CUIT no tenga prefijo duplicado
            var cuitLimpio = dto.Cuit.StartsWith("CUIT: ") ? dto.Cuit.Substring(6) : dto.Cuit;

            // Verificar si el CUIT, MP o RNP ya existe (en otro registro)
            var cuitExiste = await context.Profesionales
                .AnyAsync(p => p.Cuit == cuitLimpio && p.Id != id);
            if (cuitExiste)
                throw new ApplicationException($"Ya existe un profesional con el CUIT '{cuitLimpio}'.");

            var mpExiste = await context.Profesionales
                .AnyAsync(p => p.MP == dto.MP && p.Id != id);
            if (mpExiste)
                throw new ApplicationException($"Ya existe un profesional con la Matrícula Profesional '{dto.MP}'.");

            var rnpExiste = await context.Profesionales
                .AnyAsync(p => p.RNP == dto.RNP && p.Id != id);
            if (rnpExiste)
                throw new ApplicationException($"Ya existe un profesional con el RNP '{dto.RNP}'.");

            // Actualizar los datos
            profesional.Nombre = dto.Nombre;
            profesional.Area = dto.Area;
            profesional.Cuit = cuitLimpio;
            profesional.MP = dto.MP;
            profesional.RNP = dto.RNP;
            profesional.Telefono = dto.Telefono;

            try
            {
                context.Profesionales.Update(profesional);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Profesional_Cuit_UQ") == true)
                    throw new ApplicationException($"Ya existe un profesional con el CUIT '{cuitLimpio}'.");
                if (ex.InnerException?.Message.Contains("Profesional_MP_UQ") == true)
                    throw new ApplicationException($"Ya existe un profesional con la Matrícula Profesional '{dto.MP}'.");
                if (ex.InnerException?.Message.Contains("Profesional_RNP_UQ") == true)
                    throw new ApplicationException($"Ya existe un profesional con el RNP '{dto.RNP}'.");
                throw;
            }
        }
    }
}

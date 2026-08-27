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
                    Email = p.Email,
                    RolAsignado = p.RolAsignado,
                    EstadoRegistro = p.EstadoRegistro,
                    TipoPrestacionId = p.TipoPrestacionId,
                    TipoPrestacionNombre = p.TipoPrestacion != null ? p.TipoPrestacion.Tipo : null,
                    EsSocio = context.Socios.Any(s => s.ProfesionalId == p.Id && s.EstadoRegistro == EnumEstadoRegistro.activo)
                })
                .FirstOrDefaultAsync();
        }


        public async Task<ProfesionalListadoDTO?> SelectByCuit(string cod)
        {
            var cuitLimpio = NormalizarCuit(cod);

            ProfesionalListadoDTO? entidad = await context.Profesionales
                .Select(p => new ProfesionalListadoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Area = p.Area,
                    Cuit = p.Cuit,
                    MP = p.MP,
                    RNP = p.RNP,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    RolAsignado = p.RolAsignado
                })
                .FirstOrDefaultAsync(x => x.Cuit == cuitLimpio);

            return entidad;
        }
        public async Task<List<ProfesionalListadoDTO>> SelectListaProfesional()
        {
            var lista = await context.Profesionales
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(p => p.Area)
                .Select(p => new ProfesionalListadoDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Area = p.Area,
                    Cuit = p.Cuit,
                    MP = p.MP,
                    RNP = p.RNP,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    RolAsignado = p.RolAsignado,
                    TipoPrestacionId = p.TipoPrestacionId,
                    TipoPrestacionNombre = p.TipoPrestacion != null ? p.TipoPrestacion.Tipo : null,
                    EsSocio = context.Socios.Any(s => s.ProfesionalId == p.Id && s.EstadoRegistro == EnumEstadoRegistro.activo)
                })
                .ToListAsync();
            return lista;
        }
        public async Task<int> InsertarProfesional(ProfesionalDTO dto)
        {
            var nombreLimpio = NormalizarTexto(dto.Nombre);
            var areaLimpia = NormalizarTexto(dto.Area);
            var cuitLimpio = NormalizarCuit(dto.Cuit);
            var mpLimpia = NormalizarCodigo(dto.MP);
            var rnpLimpio = NormalizarCodigo(dto.RNP);
            var telefonoLimpio = dto.Telefono.Trim();


            var profesional = new Profesional
            {
                Nombre = NormalizarTexto(dto.Nombre),
                Area = NormalizarTexto(dto.Area),
                Cuit = NormalizarCuit(dto.Cuit),
                MP = NormalizarCodigo(dto.MP),
                RNP = NormalizarCodigo(dto.RNP),
                Telefono = dto.Telefono.Trim(),
                Email = dto.Email.Trim().ToLower(),
                RolAsignado = dto.RolAsignado,
                TipoPrestacionId = dto.TipoPrestacionId,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Profesionales.Add(profesional);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains(
                    "Profesional_Cuit_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con el CUIT '{cuitLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "Profesional_MP_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con la Matrícula Profesional '{mpLimpia}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "Profesional_RNP_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con el RNP '{rnpLimpio}'."
                    );
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

            profesional.EstadoRegistro = EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarProfesional(int id, ProfesionalDTO dto)
        {
            var profesional = await context.Profesionales
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesional == null)
                return false;


            // NORMALIZAR DATOS

            var nombreLimpio = NormalizarTexto(dto.Nombre);
            var areaLimpia = NormalizarTexto(dto.Area);
            var cuitLimpio = NormalizarCuit(dto.Cuit);
            var mpLimpia = NormalizarCodigo(dto.MP);
            var rnpLimpio = NormalizarCodigo(dto.RNP);
            var telefonoLimpio = dto.Telefono.Trim();


            // VALIDAR CUIT DUPLICADO

            var cuitExiste = await context.Profesionales
                .AnyAsync(p =>
                    p.Cuit == cuitLimpio &&
                    p.Id != id);

            if (cuitExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el CUIT '{cuitLimpio}'."
                );
            }


            // VALIDAR M.P. DUPLICADA

            var mpExiste = await context.Profesionales
                .AnyAsync(p =>
                    p.MP == mpLimpia &&
                    p.Id != id);

            if (mpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con la Matrícula Profesional '{mpLimpia}'."
                );
            }


            // VALIDAR RNP DUPLICADO


            var rnpExiste = await context.Profesionales
                .AnyAsync(p =>
                    p.RNP == rnpLimpio &&
                    p.Id != id);

            if (rnpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el RNP '{rnpLimpio}'."
                );
            }


            // ACTUALIZAR


            profesional.Nombre = nombreLimpio;
            profesional.Area = areaLimpia;
            profesional.Cuit = cuitLimpio;
            profesional.MP = mpLimpia;
            profesional.RNP = rnpLimpio;
            profesional.Telefono = telefonoLimpio;
            profesional.Email = dto.Email.Trim().ToLower();
            profesional.RolAsignado = dto.RolAsignado;
            profesional.TipoPrestacionId = dto.TipoPrestacionId;

            try
            {
                context.Profesionales.Update(profesional);

                await context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains(
                    "Profesional_Cuit_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con el CUIT '{cuitLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "Profesional_MP_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con la Matrícula Profesional '{mpLimpia}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "Profesional_RNP_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un profesional con el RNP '{rnpLimpio}'."
                    );
                }

                throw;
            }
        }

        private static string NormalizarTexto(string texto)
        {
            var cultura = new System.Globalization.CultureInfo("es-AR");

            texto = texto.Trim().ToLower(cultura);

            return cultura.TextInfo.ToTitleCase(texto);
        }

        private static string NormalizarCodigo(string texto)
        {
            return texto.Trim().ToUpperInvariant();
        }

        private static string NormalizarCuit(string cuit)
        {
            var cuitLimpio = cuit.Trim();

            if (cuitLimpio.StartsWith(
                "CUIT: ",
                StringComparison.OrdinalIgnoreCase))
            {
                cuitLimpio = cuitLimpio.Substring(6).Trim();
            }

            return cuitLimpio;
        }
    }
    
}

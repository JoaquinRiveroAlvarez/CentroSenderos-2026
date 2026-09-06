using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Validaciones;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class ProfesionalRepositorio
        : Repositorio<Profesional>, IProfesionalRepositorio
    {
        private readonly ApplicationDbContext context;

        public ProfesionalRepositorio(ApplicationDbContext context)
            : base(context)
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

                    TipoPrestacionIds =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacionId)
                            .ToList(),

                    TipoPrestacionNombres =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacion.Tipo)
                            .OrderBy(nombre => nombre)
                            .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProfesionalListadoDTO?>
            SelectByCuit(string cod)
        {
            var cuitLimpio = NormalizarCuit(cod);

            if (!CuitValidador.EsValido(cuitLimpio))
            {
                return null;
            }

            return await context.Profesionales
                .Where(p =>
                    p.Cuit
                        .Replace("-", "")
                        .Replace(".", "")
                        .Replace(" ", "") ==
                    cuitLimpio)
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

                    TipoPrestacionIds =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacionId)
                            .ToList(),

                    TipoPrestacionNombres =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacion.Tipo)
                            .OrderBy(nombre => nombre)
                            .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ProfesionalListadoDTO>>
            SelectListaProfesional()
        {
            return await context.Profesionales
                .Where(p =>
                    p.EstadoRegistro ==
                    EnumEstadoRegistro.activo)
                .OrderBy(p => p.Area)
                .ThenBy(p => p.Nombre)
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

                    TipoPrestacionIds =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacionId)
                            .ToList(),

                    TipoPrestacionNombres =
                        p.ProfesionalTipoPrestaciones
                            .Select(x =>
                                x.TipoPrestacion.Tipo)
                            .OrderBy(nombre => nombre)
                            .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                })
                .ToListAsync();
        }

        public async Task<int> InsertarProfesional(
            ProfesionalDTO dto)
        {
            var nombreLimpio =
                NormalizarTexto(dto.Nombre);

            var areaLimpia =
                NormalizarTexto(dto.Area);

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            var mpLimpia =
                NormalizarCodigo(dto.MP);

            var rnpLimpio =
                NormalizarCodigo(dto.RNP);

            var telefonoLimpio =
                dto.Telefono.Trim();

            ValidarCuit(cuitLimpio);

            var cuitExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.Cuit
                            .Replace("-", "")
                            .Replace(".", "")
                            .Replace(" ", "") ==
                        cuitLimpio);

            if (cuitExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el CUIT " +
                    $"'{CuitValidador.Formatear(cuitLimpio)}'."
                );
            }

            var mpExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.MP == mpLimpia);

            if (mpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con la " +
                    $"Matrícula Profesional '{mpLimpia}'."
                );
            }

            var rnpExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.RNP == rnpLimpio);

            if (rnpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el RNP " +
                    $"'{rnpLimpio}'."
                );
            }

            var profesional = new Profesional
            {
                Nombre = nombreLimpio,
                Area = areaLimpia,
                Cuit = cuitLimpio,
                MP = mpLimpia,
                RNP = rnpLimpio,
                Telefono = telefonoLimpio,

                Email = dto.Email
                    .Trim()
                    .ToLowerInvariant(),

                RolAsignado = dto.RolAsignado,

                EstadoRegistro =
                    EnumEstadoRegistro.activo,

                ProfesionalTipoPrestaciones =
                    dto.TipoPrestacionIds
                        .Distinct()
                        .Select(tipoPrestacionId =>
                            new ProfesionalTipoPrestacion
                            {
                                TipoPrestacionId =
                                    tipoPrestacionId
                            })
                        .ToList()
            };

            context.Profesionales.Add(profesional);

            try
            {
                await context.SaveChangesAsync();

                return profesional.Id;
            }
            catch (DbUpdateException ex)
            {
                LanzarErrorDuplicado(
                    ex,
                    cuitLimpio,
                    mpLimpia,
                    rnpLimpio
                );

                throw;
            }
        }

        public async Task<bool> ActualizarProfesional(
            int id,
            ProfesionalDTO dto)
        {
            var profesional =
                await context.Profesionales
                    .Include(p =>
                        p.ProfesionalTipoPrestaciones)
                    .FirstOrDefaultAsync(p =>
                        p.Id == id);

            if (profesional is null)
            {
                return false;
            }

            var nombreLimpio =
                NormalizarTexto(dto.Nombre);

            var areaLimpia =
                NormalizarTexto(dto.Area);

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            var mpLimpia =
                NormalizarCodigo(dto.MP);

            var rnpLimpio =
                NormalizarCodigo(dto.RNP);

            var telefonoLimpio =
                dto.Telefono.Trim();

            ValidarCuit(cuitLimpio);

            var cuitExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.Id != id &&
                        p.Cuit
                            .Replace("-", "")
                            .Replace(".", "")
                            .Replace(" ", "") ==
                        cuitLimpio);

            if (cuitExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el CUIT " +
                    $"'{CuitValidador.Formatear(cuitLimpio)}'."
                );
            }

            var mpExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.Id != id &&
                        p.MP == mpLimpia);

            if (mpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con la " +
                    $"Matrícula Profesional '{mpLimpia}'."
                );
            }

            var rnpExiste =
                await context.Profesionales
                    .AnyAsync(p =>
                        p.Id != id &&
                        p.RNP == rnpLimpio);

            if (rnpExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el RNP " +
                    $"'{rnpLimpio}'."
                );
            }

            profesional.Nombre = nombreLimpio;
            profesional.Area = areaLimpia;
            profesional.Cuit = cuitLimpio;
            profesional.MP = mpLimpia;
            profesional.RNP = rnpLimpio;
            profesional.Telefono = telefonoLimpio;

            profesional.Email = dto.Email
                .Trim()
                .ToLowerInvariant();

            profesional.RolAsignado =
                dto.RolAsignado;

            context.ProfesionalTipoPrestaciones
                .RemoveRange(
                    profesional
                        .ProfesionalTipoPrestaciones
                );

            var nuevasPrestaciones =
                dto.TipoPrestacionIds
                    .Distinct()
                    .Select(tipoPrestacionId =>
                        new ProfesionalTipoPrestacion
                        {
                            ProfesionalId =
                                profesional.Id,

                            TipoPrestacionId =
                                tipoPrestacionId
                        })
                    .ToList();

            context.ProfesionalTipoPrestaciones
                .AddRange(nuevasPrestaciones);

            try
            {
                await context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                LanzarErrorDuplicado(
                    ex,
                    cuitLimpio,
                    mpLimpia,
                    rnpLimpio
                );

                throw;
            }
        }

        public async Task<bool> DeleteProfesional(int id)
        {
            var profesional =
                await context.Profesionales
                    .FirstOrDefaultAsync(p =>
                        p.Id == id);

            if (profesional is null)
            {
                return false;
            }

            profesional.EstadoRegistro =
                EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();

            return true;
        }

        private static void ValidarCuit(string cuit)
        {
            if (!CuitValidador.EsValido(cuit))
            {
                throw new ApplicationException(
                    "El CUIT ingresado no es válido."
                );
            }
        }

        private static void LanzarErrorDuplicado(
            DbUpdateException ex,
            string cuit,
            string mp,
            string rnp)
        {
            var mensajeInterno =
                ex.InnerException?.Message ??
                string.Empty;

            if (mensajeInterno.Contains(
                "Profesional_Cuit_UQ",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el CUIT " +
                    $"'{CuitValidador.Formatear(cuit)}'."
                );
            }

            if (mensajeInterno.Contains(
                "Profesional_MP_UQ",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con la " +
                    $"Matrícula Profesional '{mp}'."
                );
            }

            if (mensajeInterno.Contains(
                "Profesional_RNP_UQ",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    $"Ya existe un profesional con el RNP " +
                    $"'{rnp}'."
                );
            }
        }

        private static string NormalizarTexto(string texto)
        {
            var cultura =
                new System.Globalization.CultureInfo(
                    "es-AR"
                );

            var textoLimpio = texto
                .Trim()
                .ToLower(cultura);

            return cultura.TextInfo
                .ToTitleCase(textoLimpio);
        }

        private static string NormalizarCodigo(string texto)
        {
            return texto
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarCuit(string cuit)
        {
            return CuitValidador.Normalizar(cuit);
        }
    }
}
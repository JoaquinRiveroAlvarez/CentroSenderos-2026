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
            var resultado = await context.Profesionales
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
                            .OrderBy(x => x.TipoPrestacion.Tipo).ThenBy(x => x.TipoPrestacionId)
                            .Select(x =>
                                x.TipoPrestacionId)
                            .ToList(),

                    TipoPrestacionNombres =
                        p.ProfesionalTipoPrestaciones
                            .OrderBy(x => x.TipoPrestacion.Tipo).ThenBy(x => x.TipoPrestacionId)
                            .Select(x =>
                                x.TipoPrestacion.Tipo)
                            .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                })
                .FirstOrDefaultAsync();
            if (resultado is not null)
            {
                resultado.MatriculasPorPrestacion = await context.ProfesionalTipoPrestaciones
                    .Where(p => p.ProfesionalId == resultado.Id)
                    .ToDictionaryAsync(p => p.TipoPrestacionId, p => p.Matricula);

                var areas = await ObtenerAreasProfesionales(new[] { resultado.Id });
                resultado.Areas = areas.GetValueOrDefault(resultado.Id) ?? new();
                if (resultado.Areas.Count > 0)
                    resultado.Area = string.Join(", ", resultado.Areas);
            }
            return resultado;
        }

        public async Task<ProfesionalListadoDTO?>SelectByCuit(string cod)
        {
            var cuitLimpio = NormalizarCuit(cod);

            if (!CuitValidador.EsValido(cuitLimpio))
            {
                return null;
            }

            var resultado = await context.Profesionales
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
                            .OrderBy(x => x.TipoPrestacion.Tipo).ThenBy(x => x.TipoPrestacionId)
                            .Select(x =>
                                x.TipoPrestacionId)
                            .ToList(),

                    TipoPrestacionNombres =
                        p.ProfesionalTipoPrestaciones
                            .OrderBy(x => x.TipoPrestacion.Tipo).ThenBy(x => x.TipoPrestacionId)
                            .Select(x =>
                                x.TipoPrestacion.Tipo)
                            .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                })
                .FirstOrDefaultAsync();
            if (resultado is not null)
            {
                resultado.MatriculasPorPrestacion = await context.ProfesionalTipoPrestaciones
                    .Where(p => p.ProfesionalId == resultado.Id)
                    .ToDictionaryAsync(p => p.TipoPrestacionId, p => p.Matricula);

                var areas = await ObtenerAreasProfesionales(new[] { resultado.Id });
                resultado.Areas = areas.GetValueOrDefault(resultado.Id) ?? new();
                if (resultado.Areas.Count > 0)
                    resultado.Area = string.Join(", ", resultado.Areas);
            }
            return resultado;
        }

        public async Task<List<ProfesionalListadoDTO>> SelectListaProfesional()
        {
            var resultado = await context.Profesionales
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
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

                    TipoPrestacionIds = p.ProfesionalTipoPrestaciones
                        .OrderBy(x => x.TipoPrestacion.Tipo)
                        .ThenBy(x => x.TipoPrestacionId)
                        .Select(x => x.TipoPrestacionId)
                        .ToList(),

                    TipoPrestacionNombres = p.ProfesionalTipoPrestaciones
                        .OrderBy(x => x.TipoPrestacion.Tipo)
                        .ThenBy(x => x.TipoPrestacionId)
                        .Select(x => x.TipoPrestacion.Tipo)
                        .ToList(),

                    EsSocio = context.Socios.Any(s =>
                        s.ProfesionalId == p.Id &&
                        s.EstadoRegistro == EnumEstadoRegistro.activo)
                })
                .ToListAsync();
            var ids = resultado.Select(p => p.Id).ToList();
            var matriculas = await context.ProfesionalTipoPrestaciones
                .Where(p => ids.Contains(p.ProfesionalId))
                .Select(p => new { p.ProfesionalId, p.TipoPrestacionId, p.Matricula }).ToListAsync();
            var areas = await ObtenerAreasProfesionales(ids);
            foreach (var profesional in resultado)
            {
                profesional.MatriculasPorPrestacion = matriculas.Where(p => p.ProfesionalId == profesional.Id)
                    .ToDictionary(p => p.TipoPrestacionId, p => p.Matricula);
                profesional.Areas = areas.GetValueOrDefault(profesional.Id) ?? new();
                if (profesional.Areas.Count > 0)
                    profesional.Area = string.Join(", ", profesional.Areas);
            }
            return resultado.OrderBy(p => p.Area).ThenBy(p => p.Nombre).ToList();
        }
        public async Task<int> InsertarProfesional(ProfesionalDTO dto)
        {
            var nombreLimpio =
                NormalizarTexto(dto.Nombre);

            var areaLimpia =
                await ObtenerPrimeraArea(dto.TipoPrestacionIds);

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            var matriculas = await ValidarMatriculas(dto);
            var mpLimpia = matriculas.Values.FirstOrDefault() ?? string.Empty;

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
                                TipoPrestacionId = tipoPrestacionId,
                                Matricula = matriculas[tipoPrestacionId],
                                EstadoRegistro = EnumEstadoRegistro.activo
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

        public async Task<bool> ActualizarProfesional(int id,ProfesionalDTO dto)
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
                await ObtenerPrimeraArea(dto.TipoPrestacionIds);

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            var matriculas = await ValidarMatriculas(dto);
            var mpLimpia = matriculas.Values.FirstOrDefault() ?? string.Empty;

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
            // El texto legado se conserva hasta retirar sus consumidores anteriores.
            if (!string.IsNullOrWhiteSpace(areaLimpia))
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

            var ids = dto.TipoPrestacionIds.Distinct().ToHashSet();
            foreach (var relacion in profesional.ProfesionalTipoPrestaciones.ToList())
            {
                if (!ids.Contains(relacion.TipoPrestacionId))
                    context.ProfesionalTipoPrestaciones.Remove(relacion);
                else
                    relacion.Matricula = matriculas[relacion.TipoPrestacionId];
            }
            foreach (var prestacionId in ids.Where(x => !profesional.ProfesionalTipoPrestaciones.Any(p => p.TipoPrestacionId == x)))
                profesional.ProfesionalTipoPrestaciones.Add(new ProfesionalTipoPrestacion
                {
                    TipoPrestacionId = prestacionId,
                    Matricula = matriculas[prestacionId],
                    EstadoRegistro = EnumEstadoRegistro.activo
                });

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

        private async Task<Dictionary<int, string>> ValidarMatriculas(ProfesionalDTO dto)
        {
            var ids = dto.TipoPrestacionIds.Distinct().ToList();
            if (ids.Count == 0) throw new ApplicationException("Seleccioná al menos una prestación.");
            var activos = await context.TipoPrestaciones.CountAsync(p => ids.Contains(p.Id) && p.EstadoRegistro == EnumEstadoRegistro.activo);
            if (activos != ids.Count) throw new ApplicationException("Una prestación no existe o está inactiva.");
            var resultado = new Dictionary<int, string>();
            foreach (var id in ids)
            {
                var matricula = dto.MatriculasPorPrestacion.GetValueOrDefault(id)?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(matricula) || matricula.Length > 30)
                    throw new ApplicationException("Ingresá una matrícula de hasta 30 caracteres para cada prestación.");
                resultado.Add(id, matricula);
            }
            return resultado;
        }

        private async Task<Dictionary<int, List<string>>> ObtenerAreasProfesionales(IEnumerable<int> profesionalIds)
        {
            var ids = profesionalIds.ToList();
            var relaciones = await context.ProfesionalTipoPrestaciones
                .Where(relacion => ids.Contains(relacion.ProfesionalId) &&
                    relacion.EstadoRegistro == EnumEstadoRegistro.activo &&
                    relacion.TipoPrestacion.TipoAreaId != null)
                .Select(relacion => new
                {
                    relacion.ProfesionalId,
                    Nombre = relacion.TipoPrestacion.TipoArea!.Tipo
                })
                .ToListAsync();

            return relaciones.GroupBy(relacion => relacion.ProfesionalId)
                .ToDictionary(grupo => grupo.Key, grupo => grupo
                    .Select(relacion => relacion.Nombre)
                    .Distinct()
                    .OrderBy(nombre => nombre)
                    .ToList());
        }

        private async Task<string> ObtenerPrimeraArea(IEnumerable<int> prestacionIds)
        {
            var ids = prestacionIds.Distinct().ToList();
            // Solo mantiene compatible la columna anterior: la lista completa se calcula al consultar.
            return await context.TipoPrestaciones
                .Where(prestacion => ids.Contains(prestacion.Id) && prestacion.TipoAreaId != null)
                .Select(prestacion => prestacion.TipoArea!.Tipo)
                .OrderBy(nombre => nombre)
                .FirstOrDefaultAsync() ?? string.Empty;
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

        private static void LanzarErrorDuplicado(DbUpdateException ex,string cuit,string mp,string rnp)
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
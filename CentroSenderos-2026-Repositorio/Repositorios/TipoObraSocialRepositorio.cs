using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using CentroSenderos_2026_Shared.Validaciones;
using System.Globalization;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoObraSocialRepositorio
        : Repositorio<TipoObraSocial>,
          ITipoObraSocialRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoObraSocialRepositorio(
            ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<TipoObraSocialDTO?> SelectPorId(int id)
        {
            return await context.TipoObrasSociales
                .Where(o => o.Id == id)
                .Select(o => new TipoObraSocialDTO
                {
                    Id = o.Id,
                    Tipo = o.Tipo,
                    Descripcion = o.Descripcion,
                    Cuit = o.Cuit ?? string.Empty,
                    EstadoRegistro = o.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoObraSocialDTO?>
            SelectByTipoObraSocial(string tipo)
        {
            var tipoLimpio = NormalizarTexto(tipo);

            return await context.TipoObrasSociales
                .Where(o => o.Tipo == tipoLimpio)
                .Select(o => new TipoObraSocialDTO
                {
                    Id = o.Id,
                    Tipo = o.Tipo,
                    Descripcion = o.Descripcion,
                    Cuit = o.Cuit ?? string.Empty,
                    EstadoRegistro = o.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoObraSocialDTO>>
            SelectListaTipoObrasocial()
        {
            return await context.TipoObrasSociales
                .Where(o =>
                    o.EstadoRegistro ==
                    EnumEstadoRegistro.activo)
                .OrderBy(o => o.Tipo)
                .Select(o => new TipoObraSocialDTO
                {
                    Id = o.Id,
                    Tipo = o.Tipo,
                    Descripcion = o.Descripcion,
                    Cuit = o.Cuit ?? string.Empty,
                    EstadoRegistro = o.EstadoRegistro
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTipoObraSocial(
            TipoObraSocialDTO dto)
        {
            var tipoLimpio =
                NormalizarTexto(dto.Tipo);

            var descripcionLimpia =
                dto.Descripcion.Trim();

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            ValidarCuit(cuitLimpio);

            var nombreExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Tipo == tipoLimpio);

            if (nombreExiste)
            {
                throw new ApplicationException(
                    $"Ya existe una obra social con el nombre '{tipoLimpio}'."
                );
            }

            var cuitExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Cuit == cuitLimpio);

            if (cuitExiste)
            {
                throw new ApplicationException(
                    $"Ya existe una obra social con el CUIT '{FormatearCuit(cuitLimpio)}'."
                );
            }

            var obraSocial = new TipoObraSocial
            {
                Tipo = tipoLimpio,
                Descripcion = descripcionLimpia,
                Cuit = cuitLimpio,
                EstadoRegistro =
                    EnumEstadoRegistro.activo
            };

            context.TipoObrasSociales.Add(obraSocial);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains(
                    "TipoObraSocial_Tipo_UQ"
                ) == true)
                {
                    throw new ApplicationException(
                        $"Ya existe una obra social con el nombre '{tipoLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "TipoObraSocial_Cuit_UQ"
                ) == true)
                {
                    throw new ApplicationException(
                        $"Ya existe una obra social con el CUIT '{FormatearCuit(cuitLimpio)}'."
                    );
                }

                throw;
            }

            return obraSocial.Id;
        }

        public async Task<bool> ActualizarTipoObraSocial(
            int id,
            TipoObraSocialDTO dto)
        {
            var obraSocial =
                await context.TipoObrasSociales
                    .FirstOrDefaultAsync(o =>
                        o.Id == id);

            if (obraSocial == null)
                return false;

            var tipoLimpio =
                NormalizarTexto(dto.Tipo);

            var descripcionLimpia =
                dto.Descripcion.Trim();

            var cuitLimpio =
                NormalizarCuit(dto.Cuit);

            ValidarCuit(cuitLimpio);

            var nombreExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Tipo == tipoLimpio &&
                        o.Id != id);

            if (nombreExiste)
            {
                throw new ApplicationException(
                    $"Ya existe una obra social con el nombre '{tipoLimpio}'."
                );
            }

            var cuitExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Cuit == cuitLimpio &&
                        o.Id != id);

            if (cuitExiste)
            {
                throw new ApplicationException(
                    $"Ya existe una obra social con el CUIT '{FormatearCuit(cuitLimpio)}'."
                );
            }

            obraSocial.Tipo = tipoLimpio;
            obraSocial.Descripcion = descripcionLimpia;
            obraSocial.Cuit = cuitLimpio;
            obraSocial.EstadoRegistro =
                dto.EstadoRegistro;

            try
            {
                context.TipoObrasSociales.Update(
                    obraSocial
                );

                await context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains(
                    "TipoObraSocial_Tipo_UQ"
                ) == true)
                {
                    throw new ApplicationException(
                        $"Ya existe una obra social con el nombre '{tipoLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains(
                    "TipoObraSocial_Cuit_UQ"
                ) == true)
                {
                    throw new ApplicationException(
                        $"Ya existe una obra social con el CUIT '{FormatearCuit(cuitLimpio)}'."
                    );
                }

                throw;
            }
        }

        public async Task<bool> DeleteTipoObraSocial(int id)
        {
            var obraSocial =
                await context.TipoObrasSociales
                    .FirstOrDefaultAsync(o =>
                        o.Id == id);

            if (obraSocial == null)
                return false;

            obraSocial.EstadoRegistro =
                EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();

            return true;
        }

        private static string NormalizarTexto(string texto)
        {
            var cultura = new CultureInfo("es-AR");

            var textoLimpio = texto
                .Trim()
                .ToLower(cultura);

            return cultura.TextInfo.ToTitleCase(
                textoLimpio
            );
        }

        private static string NormalizarCuit(string cuit)
        {
            return CuitValidador.Normalizar(cuit);
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

        private static string FormatearCuit(string cuit)
        {
            return CuitValidador.Formatear(cuit);
        }
    }
}
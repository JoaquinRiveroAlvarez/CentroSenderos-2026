using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class PlanillaRepositorio : Repositorio<Planilla>, IPlanillaRepositorio
    {
        private readonly ApplicationDbContext context;

        public PlanillaRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<List<ListadoPlanillasDTO>> SelectListaPlanillas()
        {
            // El listado no carga el contenido de los PDF.
            return await context.Planillas
                .AsNoTracking()
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new ListadoPlanillasDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    TipoPlanillaId = p.TipoPlanillaId,
                    TipoPlanillaNombre = p.TipoPlanilla != null
                        ? p.TipoPlanilla.Tipo
                        : string.Empty,
                    NombreGenerado = p.NombreGenerado,
                    TamanoBytes = p.TamanoBytes,
                    FechaSubida = p.FechaSubida
                })
                .ToListAsync();
        }

        public async Task<Planilla?> ObtenerArchivoCompleto(int id)
        {
            return await context.Planillas
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.EstadoRegistro == EnumEstadoRegistro.activo);
        }

        public async Task<int> InsertarPlanilla(
            string nombre,
            string descripcion,
            int tipoPlanillaId,
            string nombreOriginal,
            byte[] contenido)
        {
            nombre = (nombre ?? string.Empty).Trim();
            descripcion = (descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ApplicationException(
                    "Ingresá un nombre para la planilla.");

            if (nombre.Length > 150)
                throw new ApplicationException(
                    "El nombre no puede superar los 150 caracteres.");

            if (descripcion.Length > 1000)
                throw new ApplicationException(
                    "La descripción no puede superar los 1000 caracteres.");

            var tipo = await context.TipoPlanillas
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == tipoPlanillaId &&
                    t.EstadoRegistro == EnumEstadoRegistro.activo);

            if (tipo == null)
                throw new ApplicationException(
                    "Seleccioná un tipo de planilla disponible.");

            if (contenido == null || contenido.Length == 0)
                throw new ApplicationException(
                    "Seleccioná un archivo PDF con contenido.");

            const long limiteBytes = 20 * 1024 * 1024;

            if (contenido.LongLength > limiteBytes)
                throw new ApplicationException(
                    "El archivo no puede superar los 20 MB.");

            // Conservamos el nombre original sin rutas del dispositivo.
            nombreOriginal = Path.GetFileName(
                (nombreOriginal ?? string.Empty).Replace('\\', '/'));

            if (nombreOriginal.Length > 255)
                throw new ApplicationException(
                    "El nombre del archivo no puede superar los 255 caracteres.");

            if (!string.Equals(
                Path.GetExtension(nombreOriginal),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    "El archivo debe estar en formato PDF.");
            }

            // Comprobación básica de la cabecera %PDF-.
            var tieneCabeceraPdf =
                contenido.Length >= 5 &&
                contenido[0] == 0x25 &&
                contenido[1] == 0x50 &&
                contenido[2] == 0x44 &&
                contenido[3] == 0x46 &&
                contenido[4] == 0x2D;

            if (!tieneCabeceraPdf)
                throw new ApplicationException(
                    "El archivo seleccionado no tiene una cabecera PDF válida.");

            var fechaSubida = DateTime.UtcNow;

            // La fecha del nombre corresponde al horario argentino.
            var fechaArgentina = new DateTimeOffset(fechaSubida)
                .ToOffset(TimeSpan.FromHours(-3));

            var nombreArchivo = LimpiarNombreArchivo(nombre);
            var tipoArchivo = LimpiarNombreArchivo(tipo.Tipo);

            // Respetamos el límite de 255 caracteres de NombreGenerado.
            if (nombreArchivo.Length > 140)
                nombreArchivo = nombreArchivo.Substring(0, 140).TrimEnd('_');

            if (tipoArchivo.Length > 90)
                tipoArchivo = tipoArchivo.Substring(0, 90).TrimEnd('_');

            var fechaArchivo = fechaArgentina.ToString(
                "dd_MM_yyyy",
                CultureInfo.InvariantCulture);

            var nombreGenerado =
                $"{nombreArchivo}_{tipoArchivo}_{fechaArchivo}.pdf";

            var planilla = new Planilla
            {
                Nombre = nombre,
                Descripcion = descripcion,
                TipoPlanillaId = tipoPlanillaId,
                NombreOriginal = nombreOriginal,
                NombreGenerado = nombreGenerado,
                ContentType = "application/pdf",
                Contenido = contenido,
                TamanoBytes = contenido.LongLength,
                FechaSubida = fechaSubida,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Planillas.Add(planilla);
            await context.SaveChangesAsync();

            return planilla.Id;
        }

        public async Task<bool> DeletePlanilla(int id)
        {
            var planilla = await context.Planillas
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.EstadoRegistro == EnumEstadoRegistro.activo);

            if (planilla == null)
                return false;

            // Elimina el registro y su archivo PDF.
            context.Planillas.Remove(planilla);
            await context.SaveChangesAsync();

            return true;
        }

        private static string LimpiarNombreArchivo(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder();

            foreach (var caracter in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

                if (categoria == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(caracter) || caracter == '-')
                {
                    resultado.Append(caracter);
                }
                else if (char.IsWhiteSpace(caracter) || caracter == '_')
                {
                    if (resultado.Length > 0 &&
                        resultado[resultado.Length - 1] != '_')
                    {
                        resultado.Append('_');
                    }
                }
            }

            var nombreLimpio = resultado.ToString().Trim('_');

            return string.IsNullOrWhiteSpace(nombreLimpio)
                ? "Planilla"
                : nombreLimpio;
        }
    }
}
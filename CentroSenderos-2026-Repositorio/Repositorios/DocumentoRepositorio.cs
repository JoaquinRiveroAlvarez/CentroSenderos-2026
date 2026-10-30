using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System.Globalization;
using System.Text;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class DocumentoRepositorio : Repositorio<Documento>, IDocumentoRepositorio
    {
        private readonly ApplicationDbContext context;

        public DocumentoRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<DocumentoDTO> SelectPorId(int id)
        {
            return await context.Documentos
                .Where(d => d.Id == id)
                .Select(d => new DocumentoDTO
                {
                    Id = d.Id,
                    PacienteId = d.PacienteId,
                    TipoDocumentoId = d.TipoDocumentoId,
                    TipoDocumentoNombre = d.TipoDocumentos != null ? d.TipoDocumentos.Tipo : string.Empty,
                    UrlArchivo = $"/api/documento/ver/{d.Id}",
                    FechaSubida = d.FechaSubida,
                    NombreGenerado = d.NombreGenerado
                })
                .FirstAsync();
        }

        public async Task<Documento?> ObtenerArchivoCompleto(int id)
        {
            return await context.Documentos.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<int> InsertarDocumentoConArchivo(
            int pacienteId,
            int tipoDocumentoId,
            string nombreOriginal,
            string contentType,
            byte[] contenido)
        {
            var tipo = await context.TipoDocumentos.FindAsync(tipoDocumentoId);
            var paciente = await context.Pacientes.FindAsync(pacienteId);

            if (tipo == null)
                throw new InvalidOperationException($"TipoDocumentoId {tipoDocumentoId} no existe.");
            if (paciente == null)
                throw new InvalidOperationException($"PacienteId {pacienteId} no existe.");

            var tipoLimpio = SanitizarTexto(tipo.Tipo);
            var nombrePacienteLimpio = SanitizarTexto(paciente.Nombre);
            var extension = Path.GetExtension(nombreOriginal);

            // Obtener fecha y hora en zona horaria de Argentina
            DateTime ahora;
            try
            {
                ahora = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Argentina Standard Time");
            }
            catch
            {
                try
                {
                    ahora = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Argentina/Buenos_Aires");
                }
                catch
                {
                    ahora = DateTime.Now;
                }
            }

            // Formato exacto: Tipo_Paciente_dd_MM_yyyy_HHmmss.ext
            // Ejemplo: Certificado_Medico_Benjamin_Lopez_30_10_2026_194600.pdf
            var fechaHora = ahora.ToString("dd_MM_yyyy_HHmmss");
            var nombreGenerado = $"{tipoLimpio}_{nombrePacienteLimpio}_{fechaHora}{extension}";

            var documento = new Documento
            {
                PacienteId = pacienteId,
                TipoDocumentoId = tipoDocumentoId,
                NombreOriginal = nombreOriginal,
                NombreGenerado = nombreGenerado,
                ContentType = contentType,
                Contenido = contenido,
                TamanoBytes = contenido.LongLength,
                FechaSubida = DateTime.UtcNow,
                UrlArchivo = string.Empty
            };

            context.Documentos.Add(documento);
            await context.SaveChangesAsync();

            // Guardamos la URL local para visualización / descarga
            documento.UrlArchivo = $"/api/documento/ver/{documento.Id}";
            await context.SaveChangesAsync();

            return documento.Id;
        }

        public async Task<int> InsertarDocumento(DocumentoDTO dto)
        {
            var tipo = await context.TipoDocumentos.FindAsync(dto.TipoDocumentoId);
            var paciente = await context.Pacientes.FindAsync(dto.PacienteId);

            if (tipo == null)
                throw new InvalidOperationException($"TipoDocumentoId {dto.TipoDocumentoId} no existe.");
            if (paciente == null)
                throw new InvalidOperationException($"PacienteId {dto.PacienteId} no existe.");

            var tipoLimpio = SanitizarTexto(tipo.Tipo);
            var nombrePacienteLimpio = SanitizarTexto(paciente.Nombre);
            var nombreGenerado = $"{tipoLimpio}_{nombrePacienteLimpio}_{DateTime.Now:dd_MM_yyyy_HHmmss}";

            var documento = new Documento
            {
                PacienteId = dto.PacienteId,
                TipoDocumentoId = dto.TipoDocumentoId,
                UrlArchivo = dto.UrlArchivo,
                FechaSubida = DateTime.UtcNow,
                NombreGenerado = nombreGenerado
            };

            context.Documentos.Add(documento);
            await context.SaveChangesAsync();
            return documento.Id;
        }

        public async Task<bool> DeleteDocumento(int id)
        {
            var doc = await context.Documentos.FindAsync(id);
            if (doc == null) return false;

            context.Documentos.Remove(doc);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TipoDocumentoDTO>> SelectTipos()
        {
            return await context.TipoDocumentos
                .Select(t => new TipoDocumentoDTO
                {
                    Id = t.Id,
                    Tipo = t.Tipo,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();
        }

        // Remueve tildes y caracteres extraños para generar nombres de archivo 100% seguros
        private static string SanitizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "Doc";
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                    {
                        sb.Append(c);
                    }
                    else if (c == ' ')
                    {
                        sb.Append('_');
                    }
                }
            }
            return sb.ToString();
        }
    }
}
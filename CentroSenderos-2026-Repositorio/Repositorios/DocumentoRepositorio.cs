using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

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

            var extension = Path.GetExtension(nombreOriginal);
            var nombreGenerado = $"{paciente.Nombre.Replace(" ", "_")}_{tipo.Tipo.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

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
                UrlArchivo = string.Empty // Se asigna dinámicamente con el Id
            };

            context.Documentos.Add(documento);
            await context.SaveChangesAsync();

            // Guardamos la URL relativa que apunta a nuestro propio controlador
            documento.UrlArchivo = $"/api/documento/ver/{documento.Id}";
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
    }
}
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
                    TipoDocumentoNombre = d.TipoDocumentos!.Tipo,
                    UrlArchivo = d.UrlArchivo,
                    FechaSubida = d.FechaSubida,
                    NombreGenerado = d.NombreGenerado
                })
                .FirstAsync();
        }

        public async Task<int> InsertarDocumento(DocumentoDTO dto)
        {
            var tipo = await context.TipoDocumentos.FindAsync(dto.TipoDocumentoId);
            var paciente = await context.Pacientes.FindAsync(dto.PacienteId);

            if (tipo == null)
                throw new InvalidOperationException($"TipoDocumentoId {dto.TipoDocumentoId} no existe.");
            if (paciente == null)
                throw new InvalidOperationException($"PacienteId {dto.PacienteId} no existe.");

            var nombreGenerado = $"{paciente.Nombre}_{tipo.Tipo}_{DateTime.UtcNow:yyyyMMddHHmmss}_";

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

    }
}


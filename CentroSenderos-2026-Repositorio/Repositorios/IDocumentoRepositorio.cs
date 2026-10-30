using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IDocumentoRepositorio
    {
        Task<DocumentoDTO> SelectPorId(int id);
        Task<Documento?> ObtenerArchivoCompleto(int id);
        Task<int> InsertarDocumentoConArchivo(
            int pacienteId,
            int tipoDocumentoId,
            string nombreOriginal,
            string contentType,
            byte[] contenido);
        Task<bool> DeleteDocumento(int id);
        Task<List<TipoDocumentoDTO>> SelectTipos();
    }
}

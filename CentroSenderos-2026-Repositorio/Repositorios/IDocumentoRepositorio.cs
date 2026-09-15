using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IDocumentoRepositorio
    {
        Task<DocumentoDTO> SelectPorId(int id);
        Task<int> InsertarDocumento(DocumentoDTO dto); // recibe DTO con URL ya cargada
        Task<bool> DeleteDocumento(int id);
        Task<List<TipoDocumentoDTO>> SelectTipos();

    }
}

using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoDocumentoRepositorio
    {
        Task<TipoDocumentoDTO?> SelectPorId(int id);
        Task<TipoDocumentoDTO?> SelectByTipoDocumento(string tipo);
        Task<List<TipoDocumentoDTO>> SelectListaTipoDocumento();
        Task<int> InsertarTipoDocumento(TipoDocumentoDTO dto);
        Task<bool> DeleteTipoDocumento(int id);
        Task<bool> ActualizarTipoDocumento(int id, TipoDocumentoDTO dto);
    }
}
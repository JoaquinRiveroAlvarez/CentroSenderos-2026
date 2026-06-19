using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoObraSocialRepositorio
    {
        Task<TipoListadoDTO?> SelectByTipoObraSocial(string tipo);
        Task<List<TipoListadoDTO>> SelectListaTipoObrasocial();
        Task<int> InsertarTipoObraSocial(TipoDTO dto);
        Task<bool> DeleteTipoObraSocial(int id);
        Task<bool> ActualizarTipoObraSocial(int id, TipoDTO dto);
    }
}
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoAreaRepositorio
    {
        Task<TipoDTO?> SelectPorId(int id);
        Task<TipoListadoDTO?> SelectByTipoArea(string tipo);
        Task<List<TipoListadoDTO>> SelectListaTipoArea();
        Task<int> InsertarTipoArea(TipoDTO dto);
        Task<bool> ActualizarTipoArea(int id, TipoDTO dto);
        Task<bool> DeleteTipoArea(int id);
    }
}
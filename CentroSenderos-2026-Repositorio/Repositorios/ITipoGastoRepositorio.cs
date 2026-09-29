using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoGastoRepositorio
    {
        Task<TipoDTO?> SelectPorId(int id);
        Task<TipoListadoDTO?> SelectByTipoGasto(string tipo);
        Task<List<TipoListadoDTO>> SelectListaTipoGasto();
        Task<int> InsertarTipoGasto(TipoDTO dto);
        Task<bool> ActualizarTipoGasto(int id, TipoDTO dto);
        Task<bool> DeleteTipoGasto(int id);
    }
}
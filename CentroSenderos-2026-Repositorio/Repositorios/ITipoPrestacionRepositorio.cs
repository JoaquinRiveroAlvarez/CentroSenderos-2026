using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoPrestacionRepositorio
    {
        Task<TipoPrestacionListadoDTO?> SelectByCod(string cod);
        Task<List<TipoPrestacionListadoDTO>> SelectListaTipoPrestacion();
        Task<int> InsertarTipoPrestacion(TipoPrestacionDTO dto);
        Task<bool> DeleteTipoPrestacion(int id);
        Task<bool> ActualizarTipoPrestacion(int id, TipoPrestacionDTO dto);
    }
}
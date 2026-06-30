using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoDiagnosticoRepositorio
    {
        Task<TipoDTO?> SelectPorId(int id);
        Task<TipoListadoDTO?> SelectByTipoDiagnostico(string tipo);
        Task<List<TipoListadoDTO>> SelectListaTipoDiagnostico();
        Task<int> InsertarTipoDiagnostico(TipoDTO dto);
        Task<bool> ActualizarTipoDiagnostico(int id, TipoDTO dto);
        Task<bool> DeleteTipoDiagnostico(int id);
    }
}

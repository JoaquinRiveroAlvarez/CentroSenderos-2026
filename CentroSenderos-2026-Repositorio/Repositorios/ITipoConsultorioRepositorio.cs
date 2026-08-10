using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoConsultorioRepositorio
    {
        Task<TipoConsultorioDTO?> SelectPorId(int id);
        Task<TipoConsultorioListadoDTO?> SelectByTipoConsultorio(string tipo);
        Task<List<TipoConsultorioListadoDTO>> SelectListaTipoConsultorio();
        Task<int> InsertarTipoConsultorio(TipoConsultorioDTO dto);
        Task<bool> DeleteTipoConsultorio(int id);
        Task<bool> ActualizarTipoConsultorio(int id, TipoConsultorioDTO dto);
    }
}

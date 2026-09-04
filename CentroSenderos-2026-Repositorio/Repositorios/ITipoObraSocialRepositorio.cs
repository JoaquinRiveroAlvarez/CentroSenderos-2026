using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoObraSocialRepositorio
    {
        Task<TipoObraSocialDTO?> SelectPorId(int id);
        Task<TipoObraSocialDTO?> SelectByTipoObraSocial(string tipo);
        Task<List<TipoObraSocialDTO>> SelectListaTipoObrasocial();
        Task<int> InsertarTipoObraSocial(TipoObraSocialDTO dto);
        Task<bool> ActualizarTipoObraSocial(int id, TipoObraSocialDTO dto);
        Task<bool> DeleteTipoObraSocial(int id);
    }
}
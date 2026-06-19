using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoTurnoRepositorio
    {
        Task<TipoTurnoListadoDTO?> SelectByTipoTurno(string tipo);
        Task<List<TipoTurnoListadoDTO>> SelectListaTipoTurno();
        Task<int> InsertarTipoTurno(TipoTurnoDTO dto);
        Task<bool> DeleteTipoTurno(int id);
        Task<bool> ActualizarTipoTurno(int id, TipoTurnoDTO dto);
    }
}
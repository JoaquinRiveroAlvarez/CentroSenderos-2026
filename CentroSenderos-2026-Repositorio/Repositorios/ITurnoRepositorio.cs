using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITurnoRepositorio : IRepositorio<Turno>
    {
        Task<TurnoDTO?> SelectPorId(int id); // Para edición
        Task<List<TurnoListadoDTO>> SelectListaTurnos(); // Para listados
        Task<int> InsertarTurno(TurnoDTO dto);
        Task<bool> ActualizarTurno(int id, TurnoDTO dto);
        Task<bool> DeleteTurno(int id);
    }
}

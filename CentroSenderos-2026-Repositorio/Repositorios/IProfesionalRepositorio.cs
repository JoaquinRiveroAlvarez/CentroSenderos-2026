using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IProfesionalRepositorio : IRepositorio<Profesional>
    {
        Task<List<ProfesionalListadoDTO>> SelectListaProfesional();
        Task<ProfesionalListadoDTO?> SelectByCuit(string cod);
        Task<int> InsertarProfesional(ProfesionalDTO dto);
        Task<bool> ActualizarProfesional(int id, ProfesionalDTO dto);
    }
}
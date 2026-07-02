using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ISocioRepositorio : IRepositorio<Socio>
    {
        Task<SocioListadoDTO?> SelectPorId(int id);
        Task<SocioListadoDTO?> SelectByProfesionalId(int profesionalId);
        Task<List<SocioListadoDTO>> SelectListaSocios();
        Task<int> InsertarSocio(SocioDTO dto);
        Task<bool> DeleteSocio(int id);
        Task<bool> ActualizarSocio(int id, SocioDTO dto);
    }
}

using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IGastoRepositorio : IRepositorio<Gasto>
    {
        Task<int> InsertarGasto(GastoCrearDTO dto);
    }
}
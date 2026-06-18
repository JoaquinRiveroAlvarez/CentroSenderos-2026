using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IPacienteRepositorio : IRepositorio<Paciente>
    {
        Task<List<PacienteResumenDTO>> SelectListaPaciente();
        //Task<PacienteResumenDTO?> SelectPorId(int pacienteId);
        Task<int> CrearPaciente(PacienteCrearDTO dto);
        Task<bool> UpdatePacienteActualizar(int id, PacienteEditarDTO dto);
        Task<bool> Delete(int id);
    }
}

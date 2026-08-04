using CentroSenderos_2026_Shared.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ITipoPlanillaRepositorio
    {
        Task<TipoDTO?> SelectPorId(int id);
        Task<TipoListadoDTO?> SelectByTipoPlanilla(string tipo);
        Task<List<TipoListadoDTO>> SelectListaTipoPlanilla();
        Task<int> InsertarTipoPlanilla(TipoDTO dto);
        Task<bool> ActualizarTipoPlanilla(int id, TipoDTO dto);
        Task<bool> DeleteTipoPlanilla(int id);
    }
}

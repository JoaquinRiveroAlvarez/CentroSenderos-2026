using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Modelado2025_1Repositorio.Repositorios;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IGastoRepositorio : IRepositorio<Gasto>
    {
        Task<string?> AnularGasto(int id, AnularGastoDTO solicitud, string usuarioId);
        Task<int> InsertarGasto(GastoCrearDTO dto);
        Task<List<GastoListadoDTO>> SelectListaGastos();
        Task<GastosPaginaDTO> ConsultarGastos(FiltroGastosDTO filtro);
        Task<List<GastoListadoDTO>> BuscarPosiblesDuplicados(DateTime fecha, int tipoGastoId, decimal monto);
        Task<List<GastoListadoDTO>> ConsultarGastosReintegrables(int socioId);
        Task<bool> ActualizarGasto(int id,GastoEditarDTO dto,string usuarioId);
        Task<GastoCrearDTO?> SelectGastoPorId(int id);
        Task<List<GastoHistorialDTO>?> SelectHistorialGasto(int gastoId);
        Task<GastoListadoDTO?> SelectDetalleGasto(int id);

    }
}
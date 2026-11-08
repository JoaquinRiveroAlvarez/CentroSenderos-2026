using System.Collections.Generic;
using System.Threading.Tasks;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IPlanillaRepositorio
    {
        Task<List<ListadoPlanillasDTO>> SelectListaPlanillas();

        Task<Planilla?> ObtenerArchivoCompleto(int id);

        Task<int> InsertarPlanilla(
            string nombre,
            string descripcion,
            int tipoPlanillaId,
            string nombreOriginal,
            byte[] contenido);

        Task<ListadoPlanillasDTO?> ObtenerDetalle(int id);
        Task<bool> EditarPlanilla(int id, string nombre, string descripcion,
            int tipoPlanillaId, string? nombreNuevo, byte[]? nuevoContenido);

        Task<bool> DeletePlanilla(int id);
    }
}
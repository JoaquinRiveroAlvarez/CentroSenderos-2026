using System.Collections.Generic;
using System.Threading.Tasks;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface IPlanillaRepositorio
    {
        Task<List<ListadoPlanillasDTO>> SelectListaPlanillas(bool compartidas, string? propietarioPrivadoId);

        Task<Planilla?> ObtenerArchivoCompleto(int id, bool compartidas, string? propietarioPrivadoId);

        Task<int> InsertarPlanilla(
            string nombre,
            string descripcion,
            int tipoPlanillaId,
            string nombreOriginal,
            byte[] contenido, string? propietarioPrivadoId);

        Task<ListadoPlanillasDTO?> ObtenerDetalle(int id, bool compartidas, string? propietarioPrivadoId);
        Task<bool> EditarPlanilla(int id, string nombre, string descripcion,
            int tipoPlanillaId, string? nombreNuevo, byte[]? nuevoContenido, bool compartidas, string? propietarioPrivadoId);

        Task<bool> DeletePlanilla(int id, bool compartidas, string? propietarioPrivadoId);
    }
}
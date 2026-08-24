using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;

namespace Modelado2025_1.Repositorio.Seguridad;

public interface IServicioSeguridad
{
    Task<ResultadoOperacionSeguridad> AsignarRol(string email, string rol);
    Task<ResultadoOperacionSeguridad> RemoverRol(string email, string rol);
    Task<List<UsuarioDTO>> ObtenerUsuarios(string email);
}

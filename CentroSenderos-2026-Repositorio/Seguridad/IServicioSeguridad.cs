using CentroSenderos_2026_Shared.Enum;

namespace Modelado2025_1.Repositorio.Seguridad;

public interface IServicioSeguridad
{
    Task<ResultadoOperacionSeguridad> HacerAdmin (string email);
    Task<ResultadoOperacionSeguridad> RemoverAdmin(string email);
    Task<List<UsuarioDTO>> ObtenerUsuarios(string email);
}

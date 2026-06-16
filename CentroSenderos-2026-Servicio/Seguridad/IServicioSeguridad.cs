using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Servicio.Seguridad
{
    public interface IServicioSeguridad
    {
        Task<ResultadoOperacionSeguridad> HacerAdmin(string email);
        Task<ResultadoOperacionSeguridad> RemoverAdmin(string email);
        Task<List<UsuarioDTO>> ObtenerUsuarios(string email);
    }
}

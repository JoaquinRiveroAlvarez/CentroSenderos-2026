using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_Servicio.Seguridad
{
    public class ServicioSeguridad : IServicioSeguridad
    {

        //public ServicioSeguridad(AppDbContext<ApplicationUser> context,
        //                         UserManager<ApplicationUser> userManager,
        //                         IHttpContextAccessor httpContextAccessor,
        //                         IAuthorizationService authorizationService
        //{

        //}
        public Task<ResultadoOperacionSeguridad> HacerAdmin(string email)
        {
            throw new NotImplementedException();
        }

        public Task<List<UsuarioDTO>> ObtenerUsuarios(string email)
        {
            throw new NotImplementedException();
        }

        public Task<ResultadoOperacionSeguridad> RemoverAdmin(string email)
        {
            throw new NotImplementedException();
        }
    }
}

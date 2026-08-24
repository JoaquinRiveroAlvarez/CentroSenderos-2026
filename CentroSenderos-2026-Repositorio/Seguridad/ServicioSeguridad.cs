using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1.Repositorio.Seguridad;

namespace CentroSenderos_2026_Servicio.Seguridad
{
    public class ServicioSeguridad : IServicioSeguridad
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<MiUsuario> userManager;
        private readonly IHttpContextAccessor contextAccesor;
        private readonly IAuthorizationService authorizationService;

        public ServicioSeguridad(ApplicationDbContext context,
                                 UserManager<MiUsuario> userManager,
                                 IHttpContextAccessor contextAccesor,
                                 IAuthorizationService authorizationService)
        {
            this.context = context;
            this.userManager = userManager;
            this.contextAccesor = contextAccesor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Método genérico para asignar cualquier rol a un usuario.
        /// </summary>
        public async Task<ResultadoOperacionSeguridad> AsignarRol(string email, string rol)
        {
            try
            {
                var usuarioLogueado = contextAccesor.HttpContext.User;
                var resultado = await authorizationService.AuthorizeAsync(usuarioLogueado, "EsAdmin");

                if (!resultado.Succeeded)
                {
                    return ResultadoOperacionSeguridad.SinPermiso;
                }

                var usuario = await userManager.FindByEmailAsync(email);
                if (usuario == null)
                {
                    return ResultadoOperacionSeguridad.NoEncontrado;
                }

                await userManager.AddToRoleAsync(usuario, rol);
                await userManager.UpdateSecurityStampAsync(usuario);

                return ResultadoOperacionSeguridad.Exitoso;
            }
            catch
            {
                return ResultadoOperacionSeguridad.Fallido;
            }
        }

        /// <summary>
        /// Método genérico para remover cualquier rol de un usuario.
        /// </summary>
        public async Task<ResultadoOperacionSeguridad> RemoverRol(string email, string rol)
        {
            try
            {
                var usuarioLogueado = contextAccesor.HttpContext.User;
                var resultado = await authorizationService.AuthorizeAsync(usuarioLogueado, "EsAdmin");

                if (!resultado.Succeeded)
                {
                    return ResultadoOperacionSeguridad.SinPermiso;
                }

                var usuario = await userManager.FindByEmailAsync(email);
                if (usuario == null)
                {
                    return ResultadoOperacionSeguridad.NoEncontrado;
                }

                await userManager.RemoveFromRoleAsync(usuario, rol);
                await userManager.UpdateSecurityStampAsync(usuario);

                return ResultadoOperacionSeguridad.Exitoso;
            }
            catch
            {
                return ResultadoOperacionSeguridad.Fallido;
            }
        }

        /// <summary>
        /// Listado de usuarios con sus roles.
        /// </summary>
        public async Task<List<UsuarioDTO>> ObtenerUsuarios(string email)
        {
            var usuarios = await context.Users
                .Where(u => string.IsNullOrEmpty(email) || u.Email!.Contains(email))
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Email = u.Email!,
                    Nombre = u.UserName!
                }).ToListAsync();

            // Cargar roles de cada usuario
            foreach (var u in usuarios)
            {
                var usuario = await userManager.FindByEmailAsync(u.Email);
                if (usuario != null)
                {
                    var roles = await userManager.GetRolesAsync(usuario);
                    u.Roles = roles.ToList();
                }
            }

            return usuarios;
        }

        // Métodos específicos (si querés mantenerlos por comodidad)
        public Task<ResultadoOperacionSeguridad> HacerAdmin(string email) =>
            AsignarRol(email, "admin");

        public Task<ResultadoOperacionSeguridad> RemoverAdmin(string email) =>
            RemoverRol(email, "admin");
    }
}

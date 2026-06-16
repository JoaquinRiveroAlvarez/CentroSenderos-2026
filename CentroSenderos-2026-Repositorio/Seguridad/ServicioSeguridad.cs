using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Modelado2025_1.Repositorio.Seguridad;

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

    public async Task<ResultadoOperacionSeguridad> HacerAdmin(string email)
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

            await userManager.AddToRoleAsync(usuario, "admin");
            await userManager.UpdateSecurityStampAsync(usuario);

            return ResultadoOperacionSeguridad.Exitoso;
        }
        catch (Exception e)
        {
            return ResultadoOperacionSeguridad.Fallido;
        }
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

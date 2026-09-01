using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EsAdmin")] // solo admins pueden usarlo
    public class SeguridadController : ControllerBase
    {
        private readonly UserManager<MiUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeguridadController(UserManager<MiUsuario> userManager,
                                   RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/seguridad/usuarios
        [HttpGet("usuarios")]
        public async Task<ActionResult<List<UsuarioDTO>>> ObtenerUsuarios()
        {
            try
            {
                var usuarios = _userManager.Users.ToList();
                var lista = new List<UsuarioDTO>();

                foreach (var u in usuarios)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    lista.Add(new UsuarioDTO
                    {
                        Id = u.Id,
                        Email = u.Email ?? string.Empty,
                        Nombre = u.UserName ?? string.Empty,
                        Roles = roles.ToList()
                    });
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno en SeguridadController: {ex.Message}");
            }
        }



        // POST: api/seguridad/asignarRol
        [HttpPost("asignarRol")]
        public async Task<ActionResult<string>> AsignarRol(
    [FromBody] RolAsignacionDTO dto)
        {
            var usuario = await _userManager
                .FindByEmailAsync(dto.Email);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (!await _roleManager.RoleExistsAsync(dto.Rol))
            {
                return BadRequest(
                    $"El rol '{dto.Rol}' no existe."
                );
            }

            var yaTieneRol = await _userManager
                .IsInRoleAsync(usuario, dto.Rol);

            if (yaTieneRol)
            {
                return Ok(
                    "El usuario ya tiene asignado ese rol."
                );
            }

            var resultado = await _userManager
                .AddToRoleAsync(usuario, dto.Rol);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(
                    " ",
                    resultado.Errors.Select(error =>
                        error.Description)
                );

                return BadRequest(errores);
            }

            return Ok("Rol asignado correctamente.");
        }

        // POST: api/seguridad/removerRol
        [HttpPost("removerRol")]
        public async Task<ActionResult> RemoverRol([FromBody] RolAsignacionDTO dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario == null) return NotFound("Usuario no encontrado");

            var resultado = await _userManager.RemoveFromRoleAsync(usuario, dto.Rol);
            if (!resultado.Succeeded) return BadRequest(resultado.Errors);

            return Ok();
        }
    }
}

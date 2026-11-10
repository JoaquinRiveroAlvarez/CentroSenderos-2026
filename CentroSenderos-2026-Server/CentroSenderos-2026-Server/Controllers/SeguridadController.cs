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
        public async Task<ActionResult<string>> AsignarRol([FromBody] RolAsignacionDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("El email es requerido.");
            }

            var emailLimpio = dto.Email.Trim().ToLowerInvariant();

            // Si se especificó un rol, verificar que exista en AspNetRoles
            if (!string.IsNullOrWhiteSpace(dto.Rol) && !await _roleManager.RoleExistsAsync(dto.Rol))
            {
                return BadRequest($"El rol '{dto.Rol}' no existe en el sistema.");
            }

            var usuario = await _userManager.FindByEmailAsync(emailLimpio);

            // Si el usuario aún no existe en AspNetUsers, lo creamos para que el profesional
            // quede inmediatamente registrado en el sistema con su cuenta y rol correspondiente
            if (usuario == null)
            {
                usuario = new MiUsuario
                {
                    UserName = emailLimpio,
                    Email = emailLimpio,
                    EmailConfirmed = true
                };

                var resultadoCrear = await _userManager.CreateAsync(usuario);
                if (!resultadoCrear.Succeeded)
                {
                    var erroresCrear = string.Join(" ", resultadoCrear.Errors.Select(e => e.Description));
                    return BadRequest($"No se pudo crear la cuenta de usuario para el profesional: {erroresCrear}");
                }
            }

            var rolesDelSistema = new[] { "admin", "equipo", "profesional", "AdministradorEquipo" };
            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            // Si viene un rol vacío, removemos los roles anteriores del sistema
            if (string.IsNullOrWhiteSpace(dto.Rol))
            {
                var rolesARemover = rolesActuales
                    .Where(r => rolesDelSistema.Contains(r, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (rolesARemover.Any())
                {
                    await _userManager.RemoveFromRolesAsync(usuario, rolesARemover);
                    await _userManager.UpdateSecurityStampAsync(usuario);
                }

                return Ok("Rol removido correctamente.");
            }

            // Remover roles del sistema anteriores distintos al nuevo rol asignado
            var rolesAEliminar = rolesActuales
                .Where(r => rolesDelSistema.Contains(r, StringComparer.OrdinalIgnoreCase) &&
                            !string.Equals(r, dto.Rol, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (rolesAEliminar.Any())
            {
                await _userManager.RemoveFromRolesAsync(usuario, rolesAEliminar);
            }

            if (!await _userManager.IsInRoleAsync(usuario, dto.Rol))
            {
                var resultado = await _userManager.AddToRoleAsync(usuario, dto.Rol);
                if (!resultado.Succeeded)
                {
                    var errores = string.Join(" ", resultado.Errors.Select(error => error.Description));
                    return BadRequest(errores);
                }
            }

            await _userManager.UpdateSecurityStampAsync(usuario);

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

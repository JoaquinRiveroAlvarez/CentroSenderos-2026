using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Servicio.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CentroSenderos_2026_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EsAdmin")]
    public class SeguridadController : ControllerBase
    {
        private readonly UserManager<MiUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SeguridadController(UserManager<MiUsuario> userManager,
                                   RoleManager<IdentityRole> roleManager,
                                   ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        private static readonly string[] Roles = ["Socio", "admin", "Calendario", "equipo", "profesional"];

        private string? BuscarRol(string? rol) =>
            Roles.FirstOrDefault(r => string.Equals(r, rol?.Trim(), StringComparison.OrdinalIgnoreCase));

        private bool PuedeGestionar(string rol) =>
            rol is not ("Socio" or "admin") || User.IsInRole("Socio") || User.IsInRole("admin");

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
                        Roles = roles.Where(r => BuscarRol(r) is not null).ToList()
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

            var rol = BuscarRol(dto.Rol);
            if (rol is null || !await _roleManager.RoleExistsAsync(rol))
                return BadRequest("Seleccioná un rol válido.");
            if (!PuedeGestionar(rol))
                return Forbid();

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

            var resultado = await AsignacionRoles.AsignarAsync(_context, _userManager, usuario, rol);
            if (!resultado.Succeeded)
            {
                var errores = string.Join(" ", resultado.Errors.Select(error => error.Description));
                return BadRequest(errores);
            }

            return Ok("Rol asignado correctamente.");
        }

        [HttpPost("restablecerContrasena")]
        [Authorize(Policy = "GestionaRolesPrivilegiados")]
        public async Task<IActionResult> RestablecerContrasena([FromBody] RestablecerContrasenaDTO dto)
        {
            var usuario = await _userManager.FindByIdAsync(dto.UsuarioId);
            if (usuario is null)
                return NotFound("Usuario no encontrado.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, token, dto.NuevaContrasena);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    var campo = error.Code.StartsWith("Password", StringComparison.Ordinal)
                        ? nameof(RestablecerContrasenaDTO.NuevaContrasena)
                        : string.Empty;
                    ModelState.AddModelError(campo, error.Description);
                }

                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        // POST: api/seguridad/removerRol
        [HttpPost("removerRol")]
        public async Task<ActionResult> RemoverRol([FromBody] RolAsignacionDTO dto)
        {
            var rol = BuscarRol(dto.Rol);
            if (rol is null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Seleccioná una cuenta y un rol válido.");
            if (!PuedeGestionar(rol))
                return Forbid();

            var usuario = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (usuario == null) return NotFound("Usuario no encontrado");

            var resultado = await AsignacionRoles.QuitarAsync(_context, _userManager, usuario, rol);
            if (!resultado.Succeeded) return BadRequest(string.Join(" ", resultado.Errors.Select(error => error.Description)));

            return Ok();
        }
    }
}

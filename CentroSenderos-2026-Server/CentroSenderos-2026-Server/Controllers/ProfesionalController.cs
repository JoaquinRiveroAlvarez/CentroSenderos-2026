using CentroSenderos_2026_BD;
using Microsoft.AspNetCore.Identity;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CentroSenderos_2026_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profesional")]
    public class ProfesionalController : ControllerBase
    {
        private readonly IProfesionalRepositorio repositorio;

        private readonly ApplicationDbContext context;
        private readonly UserManager<MiUsuario> usuarios;

        public ProfesionalController(IProfesionalRepositorio repositorio, ApplicationDbContext context, UserManager<MiUsuario> usuarios)
        {
            this.repositorio = repositorio;
            this.context = context;
            this.usuarios = usuarios;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProfesionalDTO>> GetById(int id)
        {
            var entidad = await repositorio.SelectPorId(id);
            if (entidad == null) return NotFound($"No existe profesional con id {id}.");
            return Ok(entidad);
        }


        [HttpGet("ListaProfesional")]
        public async Task<IActionResult> GetListaProfesionales()
        {
            var lista = await repositorio.SelectListaProfesional();
            if (lista == null || !lista.Any())
                return NotFound("No hay profesionales registrados.");

            return Ok(lista);
        }

        [HttpGet("Cuit/{cod}")]
        public async Task<ActionResult<ProfesionalListadoDTO>> SelectByCuit(string cod)
        {
            var tipoProvincia = await repositorio.SelectByCuit(cod);
            if (tipoProvincia is null)
            {
                return NotFound($"No existe el registro con el cuit: {cod}.");
            }

            return Ok(tipoProvincia);
        }

        [HttpPost]
        public async Task<IActionResult> PostEvento([FromBody] ProfesionalDTO dto)
        {
            try
            {
                var rol = string.IsNullOrWhiteSpace(dto.RolAsignado) ? "profesional" : dto.RolAsignado;
                if (rol != "profesional" && !User.IsInRole("admin") && !User.IsInRole("AdministradorEquipo"))
                    return Forbid();
                var email = dto.Email.Trim().ToLowerInvariant();
                if (await usuarios.FindByEmailAsync(email) != null)
                    return BadRequest(new { mensaje = "Ya existe un usuario con ese email." });
                await using var transaccion = await context.Database.BeginTransactionAsync();
                var usuario = new MiUsuario { UserName = email, Email = email, EmailConfirmed = true };
                // Contraseña inicial solicitada, usando el hash de Identity sin cambiar la política global.
                usuario.PasswordHash = usuarios.PasswordHasher.HashPassword(usuario, "123456");
                var creado = await usuarios.CreateAsync(usuario);
                if (!creado.Succeeded) throw new ApplicationException(string.Join(" ", creado.Errors.Select(e => e.Description)));
                var asignado = await usuarios.AddToRoleAsync(usuario, rol);
                if (!asignado.Succeeded) throw new ApplicationException(string.Join(" ", asignado.Errors.Select(e => e.Description)));
                dto.RolAsignado = rol;
                int id = await repositorio.InsertarProfesional(dto);
                await transaccion.CommitAsync();
                return Ok(id);
            }
            catch (ApplicationException ex)
            {
                // Esto devuelve el mensaje controlado al cliente
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ProfesionalDTO dto)
        {
            try
            {
                var resultado = await repositorio.ActualizarProfesional(id, dto);
                if (!resultado)
                {
                    return NotFound($"No existe el profesional con el id: {id}.");
                }

                return Ok($"El registro con el id: {id} fue actualizado correctamente.");
            }
            catch (ApplicationException ex)
            {
                // Esto devuelve el mensaje controlado al cliente
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Errores no esperados
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProfesional(int id)
        {
            var resultado = await repositorio.DeleteProfesional(id);
            if (!resultado)
            {
                return BadRequest("Datos no validos");
            }
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }
    }
}

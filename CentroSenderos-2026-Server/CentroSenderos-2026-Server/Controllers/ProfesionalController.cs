using Microsoft.EntityFrameworkCore;
using CentroSenderos_2026_Servicio.Seguridad;
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
    [Authorize(Policy = "EsProfesional")]
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
            if (!AlcanceProfesional.TieneAccesoCompleto(User)
                && !await AlcanceProfesional.Profesionales(context, User).ContainsAsync(id))
                return NotFound();

            var entidad = await repositorio.SelectPorId(id);
            if (entidad == null) return NotFound($"No existe profesional con id {id}.");
            return Ok(entidad);
        }


        [HttpGet("ListaProfesional")]
        public async Task<IActionResult> GetListaProfesionales([FromQuery] bool bajas = false)
        {
            if (bajas && !AlcanceProfesional.TieneAccesoCompleto(User)) return Forbid();
            var lista = await repositorio.SelectListaProfesional(bajas);
            if (lista is not null && !AlcanceProfesional.TieneAccesoCompleto(User))
            {
                var permitidos = (await AlcanceProfesional.Profesionales(context, User).ToListAsync()).ToHashSet();
                lista = lista.Where(p => permitidos.Contains(p.Id)).ToList();
            }
            if (lista == null || !lista.Any())
                return NotFound("No hay profesionales registrados.");

            return Ok(lista);
        }

        [HttpGet("Cuit/{cod}")]
        public async Task<ActionResult<ProfesionalListadoDTO>> SelectByCuit(string cod)
        {
            var tipoProvincia = await repositorio.SelectByCuit(cod);
            if (tipoProvincia is null || (!AlcanceProfesional.TieneAccesoCompleto(User)
                && !await AlcanceProfesional.Profesionales(context, User).ContainsAsync(tipoProvincia.Id)))
            {
                return NotFound($"No existe el registro con el cuit: {cod}.");
            }

            return Ok(tipoProvincia);
        }

        [HttpPost]
        [Authorize(Policy = "EsAdmin")]
        public async Task<IActionResult> PostEvento([FromBody] ProfesionalDTO dto)
        {
            try
            {
                var rol = string.IsNullOrWhiteSpace(dto.RolAsignado) ? "profesional" : dto.RolAsignado;
                if (rol is not ("profesional" or "equipo"))
                    return BadRequest("Seleccioná un rol válido.");
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
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> Put(int id, ProfesionalDTO dto)
        {
            try
            {
                await using var transaccion = await context.Database.BeginTransactionAsync();
                var ficha = await context.Profesionales.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (ficha is null) return NotFound();
                var cuenta = await usuarios.FindByEmailAsync(ficha.Email);
                var correo = dto.Email.Trim().ToLowerInvariant();
                if (!string.Equals(ficha.Email, correo, StringComparison.OrdinalIgnoreCase))
                {
                    if (cuenta is not null && await CuentaPrivilegiada(cuenta) && !PuedeGestionarPrivilegiados)
                        return Forbid();
                    if (await usuarios.FindByEmailAsync(correo) is not null)
                        return BadRequest("Ya existe una cuenta con ese correo.");
                    if (cuenta is not null)
                    {
                        cuenta.Email = correo;
                        cuenta.UserName = correo;
                        var cambio = await usuarios.UpdateAsync(cuenta);
                        if (!cambio.Succeeded) return BadRequest(cambio.Errors);
                    }
                }

                dto.RolAsignado = ficha.RolAsignado;
                var resultado = await repositorio.ActualizarProfesional(id, dto);
                if (!resultado)
                {
                    return NotFound($"No existe el profesional con el id: {id}.");
                }

                await transaccion.CommitAsync();
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
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> DeleteProfesional(int id)
        {
            await using var transaccion = await context.Database.BeginTransactionAsync();
            var ficha = await context.Profesionales.FirstOrDefaultAsync(p => p.Id == id);
            if (ficha is null) return NotFound();

            var cuenta = await usuarios.FindByEmailAsync(ficha.Email);
            if (cuenta is not null)
            {
                if (await CuentaPrivilegiada(cuenta) && !PuedeGestionarPrivilegiados)
                    return Forbid();

                var roles = await usuarios.GetRolesAsync(cuenta);
                var rolesDeFicha = roles.Where(r => r is "profesional" or "equipo").ToArray();
                if (rolesDeFicha.Length > 0)
                {
                    var resultado = await usuarios.RemoveFromRolesAsync(cuenta, rolesDeFicha);
                    if (!resultado.Succeeded) return BadRequest(resultado.Errors);
                    if (rolesDeFicha.Contains("profesional"))
                        await AsignacionRoles.BorrarPrivadasAsync(context, cuenta);
                }
            }

            ficha.RolAsignado = string.Empty;
            if (!await repositorio.DeleteProfesional(id))
                return BadRequest("Datos no validos");

            await transaccion.CommitAsync();
            return Ok($"El registro con el id: {id} fue eliminado correctamente.");
        }

        [HttpPut("{id:int}/reactivar")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var ficha = await context.Profesionales.FirstOrDefaultAsync(p => p.Id == id);
            if (ficha is null) return NotFound();

            ficha.EstadoRegistro = EnumEstadoRegistro.activo;
            await context.SaveChangesAsync();
            return NoContent();
        }

        private bool PuedeGestionarPrivilegiados => User.IsInRole("Socio") || User.IsInRole("admin");

        private async Task<bool> CuentaPrivilegiada(MiUsuario cuenta) =>
            await usuarios.IsInRoleAsync(cuenta, "Socio") || await usuarios.IsInRoleAsync(cuenta, "admin");
    }
}

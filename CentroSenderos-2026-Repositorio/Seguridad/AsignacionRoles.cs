using CentroSenderos_2026_BD;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Servicio.Seguridad;

public static class AsignacionRoles
{
    public static async Task<IdentityResult> AsignarAsync(
        ApplicationDbContext context, UserManager<MiUsuario> usuarios, MiUsuario usuario, string rol)
    {
        var rolIncompatible = usuarios.NormalizeName(rol) switch
        {
            "EQUIPO" => "profesional",
            "PROFESIONAL" => "equipo",
            _ => null
        };

        await using var transaccion = await context.Database.BeginTransactionAsync();
        if (rolIncompatible is not null && await usuarios.IsInRoleAsync(usuario, rolIncompatible))
        {
            var remocion = await usuarios.RemoveFromRoleAsync(usuario, rolIncompatible);
            if (!remocion.Succeeded)
                return remocion;
            if (rolIncompatible == "profesional")
                await BorrarPrivadasAsync(context, usuario);
        }

        if (!await usuarios.IsInRoleAsync(usuario, rol))
        {
            var asignacion = await usuarios.AddToRoleAsync(usuario, rol);
            if (!asignacion.Succeeded)
                return asignacion;
        }

        await ActualizarFichaAsync(context, usuarios, usuario);
        await transaccion.CommitAsync();
        return IdentityResult.Success;
    }

    public static async Task<IdentityResult> QuitarAsync(
        ApplicationDbContext context, UserManager<MiUsuario> usuarios, MiUsuario usuario, string rol)
    {
        await using var transaccion = await context.Database.BeginTransactionAsync();
        var resultado = await usuarios.RemoveFromRoleAsync(usuario, rol);
        if (!resultado.Succeeded)
            return resultado;

        if (usuarios.NormalizeName(rol) == "PROFESIONAL")
            await BorrarPrivadasAsync(context, usuario);

        await ActualizarFichaAsync(context, usuarios, usuario);
        await transaccion.CommitAsync();
        return IdentityResult.Success;
    }

    private static async Task ActualizarFichaAsync(
        ApplicationDbContext context, UserManager<MiUsuario> usuarios, MiUsuario usuario)
    {
        if (usuario.NormalizedEmail is null)
            return;

        var roles = await usuarios.GetRolesAsync(usuario);
        var rol = new[] { "equipo", "profesional", "Socio", "admin", "Calendario" }
            .FirstOrDefault(r => roles.Contains(r, StringComparer.OrdinalIgnoreCase));

        await context.Profesionales.Where(p => p.Email.ToUpper() == usuario.NormalizedEmail)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.RolAsignado, rol));
    }

    public static Task<int> BorrarPrivadasAsync(ApplicationDbContext context, MiUsuario usuario) =>
        context.Planillas.Where(p => p.PropietarioPrivadoId == usuario.Id).ExecuteDeleteAsync();
}

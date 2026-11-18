using System.Security.Claims;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Server.Controllers;

internal static class AlcanceProfesional
{
    public static bool TieneAccesoCompleto(ClaimsPrincipal usuario) =>
        usuario.IsInRole("Socio") || usuario.IsInRole("admin") || usuario.IsInRole("Calendario");

    public static IQueryable<int> Profesionales(ApplicationDbContext context, ClaimsPrincipal usuario)
    {
        var profesionales = context.Profesionales.Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo);
        if (TieneAccesoCompleto(usuario))
            return profesionales.Select(p => p.Id);

        var cuentaId = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuario.IsInRole("equipo"))
        {
            return from profesional in profesionales
                   join cuenta in context.Users on profesional.Email.ToUpper() equals cuenta.NormalizedEmail
                   join asignacion in context.UserRoles on cuenta.Id equals asignacion.UserId
                   join rol in context.Roles on asignacion.RoleId equals rol.Id
                   where rol.NormalizedName == "EQUIPO"
                   select profesional.Id;
        }

        return from profesional in profesionales
               join cuenta in context.Users on profesional.Email.ToUpper() equals cuenta.NormalizedEmail
               where cuenta.Id == cuentaId
               select profesional.Id;
    }
}

using System.Security.Claims;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.Enum;

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

        return usuario.IsInRole("equipo")
            ? PorRol(context, "EQUIPO")
            : Propios(context, usuario);
    }

    public static IQueryable<int> PorRol(ApplicationDbContext context, string rol) =>
        from profesional in context.Profesionales
        join cuenta in context.Users on profesional.Email.ToUpper() equals cuenta.NormalizedEmail
        join asignacion in context.UserRoles on cuenta.Id equals asignacion.UserId
        join rolCuenta in context.Roles on asignacion.RoleId equals rolCuenta.Id
        where profesional.EstadoRegistro == EnumEstadoRegistro.activo && rolCuenta.NormalizedName == rol
        select profesional.Id;

    public static IQueryable<int> Propios(ApplicationDbContext context, ClaimsPrincipal usuario)
    {
        var cuentaId = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        return from profesional in context.Profesionales
               join cuenta in context.Users on profesional.Email.ToUpper() equals cuenta.NormalizedEmail
               where cuenta.Id == cuentaId && profesional.EstadoRegistro == EnumEstadoRegistro.activo
               select profesional.Id;
    }

    public static IQueryable<Turno> TurnosPermitidos(ApplicationDbContext context, ClaimsPrincipal usuario, bool incluirMixtos = false)
    {
        if (TieneAccesoCompleto(usuario))
            return context.Turnos;

        var ids = Profesionales(context, usuario);
        var habituales = context.Turnos.Where(t => t.TurnoProfesionales.Any()
            && t.TurnoProfesionales.All(p => ids.Contains(p.ProfesionalId)));
        return incluirMixtos ? habituales.Union(TurnosMixtosPropios(context, usuario)) : habituales;
    }

    public static IQueryable<Turno> TurnosMixtosPropios(ApplicationDbContext context, ClaimsPrincipal usuario)
    {
        var equipo = PorRol(context, "EQUIPO");
        var externos = PorRol(context, "PROFESIONAL");
        var propios = Propios(context, usuario);
        return context.Turnos.Where(t => t.TurnoProfesionales.Any(p => propios.Contains(p.ProfesionalId))
            && t.TurnoProfesionales.Any(p => equipo.Contains(p.ProfesionalId))
            && t.TurnoProfesionales.Any(p => externos.Contains(p.ProfesionalId)));
    }
}

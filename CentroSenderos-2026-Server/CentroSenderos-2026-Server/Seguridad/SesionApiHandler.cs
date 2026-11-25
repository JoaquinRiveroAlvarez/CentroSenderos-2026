using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace CentroSenderos_2026_Server.Seguridad;

public sealed class SesionApiHandler : DelegatingHandler
{
    private static readonly PathString[] RutasProtegidas =
    [
        new("/api/paciente"),
        new("/api/profesional"),
        new("/api/turno"),
        new("/api/seguridad"),
        new("/api/socio"),
        new("/api/tipoarea"),
        new("/api/tipoconsultorio"),
        new("/api/tipodiagnostico"),
        new("/api/tipodocumento"),
        new("/api/tipogasto"),
        new("/api/tipoobrasocial"),
        new("/api/tipoplanillas"),
        new("/api/tipoprestacion"),
        new("/api/tipoturno")
    ];

    private readonly AuthenticationStateProvider autenticacion;
    private readonly Uri destino;
    private readonly string nombreCookie;
    private readonly string? cookieSesion;

    public SesionApiHandler(
        AuthenticationStateProvider autenticacion,
        IHttpContextAccessor accesoHttp,
        IOptionsMonitor<CookieAuthenticationOptions> opciones,
        Uri destino)
    {
        this.autenticacion = autenticacion;
        this.destino = destino;

        var cookie = opciones.Get(IdentityConstants.ApplicationScheme);
        nombreCookie = cookie.Cookie.Name
            ?? throw new InvalidOperationException("Falta el nombre de la cookie de Identity.");

        var contexto = accesoHttp.HttpContext;
        if (contexto is not null)
        {
            var origen = new Uri(contexto.Request.Scheme + "://" + contexto.Request.Host);

            if (MismoOrigen(origen, destino))
            {
                cookieSesion = cookie.CookieManager.GetRequestCookie(contexto, nombreCookie);
            }
        }

        InnerHandler = new HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        if (uri is null || !MismoOrigen(uri, destino))
        {
            throw new InvalidOperationException(
                "El cliente interno solo puede llamar al origen de su API.");
        }

        var path = new PathString(uri.AbsolutePath);
        var protegida = RutasProtegidas.Any(ruta => path.StartsWithSegments(ruta));

        if (protegida)
        {
            var estado = await autenticacion.GetAuthenticationStateAsync();

            if (estado.User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(cookieSesion))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request
                };
            }

            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", nombreCookie + "=" + cookieSesion);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static bool MismoOrigen(Uri izquierda, Uri derecha) =>
        izquierda.Scheme.Equals(derecha.Scheme, StringComparison.OrdinalIgnoreCase)
        && izquierda.Host.Equals(derecha.Host, StringComparison.OrdinalIgnoreCase)
        && izquierda.Port == derecha.Port;
}
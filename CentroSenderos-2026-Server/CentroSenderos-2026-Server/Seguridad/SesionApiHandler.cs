using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace CentroSenderos_2026_Server.Seguridad;

/// <summary>
/// Reenvía únicamente la cookie de Identity de este ámbito hacia la propia API.
/// Cada circuito obtiene su propio handler: no hay cookies compartidas entre usuarios.
/// </summary>
public sealed class SesionApiHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider autenticacion;
    private readonly Uri destino;
    private readonly string nombreCookie;
    private readonly string? cookieSesion;

    public SesionApiHandler(AuthenticationStateProvider autenticacion,
        IHttpContextAccessor accesoHttp,
        IOptionsMonitor<CookieAuthenticationOptions> opciones,
        Uri destino)
    {
        this.autenticacion = autenticacion;
        this.destino = destino;
        var cookie = opciones.Get(IdentityConstants.ApplicationScheme);
        nombreCookie = cookie.Cookie.Name
            ?? throw new InvalidOperationException("Falta el nombre de la cookie de Identity.");

        // Capturar el valor durante la creación del ámbito HTTP/circuito.
        // No consultar HttpContext durante eventos posteriores de Blazor.
        var contexto = accesoHttp.HttpContext;
        if (contexto != null)
        {
            var origen = new Uri(contexto.Request.Scheme + "://" + contexto.Request.Host);
            if (MismoOrigen(origen, destino))
                cookieSesion = cookie.CookieManager.GetRequestCookie(contexto, nombreCookie);
        }

        // No guardar cookies de respuestas ni seguir redirecciones con credenciales.
        InnerHandler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        if (uri == null || !MismoOrigen(uri, destino))
            throw new InvalidOperationException("El cliente interno solo puede llamar al origen de su API.");

        // Las API restantes conservan su comportamiento mientras se auditan por separado.
        var path = uri.AbsolutePath;
        var protegida = path.Equals("/api/paciente", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/paciente/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/profesional", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/profesional/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/turno", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/turno/", StringComparison.OrdinalIgnoreCase);
        if (protegida)
        {
            var estado = await autenticacion.GetAuthenticationStateAsync();
            if (estado.User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(cookieSesion))
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = request };
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

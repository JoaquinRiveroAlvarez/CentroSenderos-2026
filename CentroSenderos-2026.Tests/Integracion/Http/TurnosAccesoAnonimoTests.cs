using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosAccesoAnonimoTests
{
    #region Acceso sin sesión

    [Theory]
    [Trait("Categoria", "Http")]
    [Trait("Categoria", "Seguridad")]
    [InlineData("api/turno/-1")]
    [InlineData("api/turno/ListaTurnos")]
    public async Task ConsultaSinSesion_DebeExigirAutenticacion(
        string ruta)
    {
        var direccion = Environment.GetEnvironmentVariable(
            "CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(
            Uri.TryCreate(direccion, UriKind.Absolute, out var baseUrl),
            "La dirección debe ser una URL absoluta.");

        Assert.True(
            baseUrl!.IsLoopback && baseUrl.Scheme == "https",
            "Estas pruebas solo admiten un servidor local HTTPS.");

        using var handler = new HttpClientHandler
        {
            UseCookies = false,
            UseDefaultCredentials = false,
            AllowAutoRedirect = false
        };

        using var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                baseUrl.AbsoluteUri.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        using var respuesta = await cliente.GetAsync(ruta);

        var bloqueado =
            respuesta.StatusCode == HttpStatusCode.Unauthorized ||
            respuesta.StatusCode == HttpStatusCode.Forbidden;

        var codigo = (int)respuesta.StatusCode;

        if (codigo is 301 or 302 or 303 or 307 or 308)
        {
            var destino = respuesta.Headers.Location;

            if (destino is not null)
            {
                var login = destino.IsAbsoluteUri
                    ? destino
                    : new Uri(baseUrl, destino);

                bloqueado =
                    login.GetLeftPart(UriPartial.Authority) ==
                        baseUrl.GetLeftPart(UriPartial.Authority) &&
                    login.AbsolutePath.TrimEnd('/').Equals(
                        "/Account/Login",
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        Assert.True(
            bloqueado,
            $"GET /{ruta} sin sesión devolvió HTTP {codigo}. " +
            "Se esperaba 401, 403 o una redirección al login.");
    }

    #endregion
}
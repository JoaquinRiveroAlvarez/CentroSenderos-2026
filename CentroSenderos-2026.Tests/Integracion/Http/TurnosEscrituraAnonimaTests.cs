using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosEscrituraAnonimaTests
{
    #region Escritura sin sesión

    [Theory]
    [Trait("Categoria", "Http")]
    [Trait("Categoria", "Seguridad")]
    [InlineData("POST", "/api/turno")]
    [InlineData("PUT", "/api/turno/-1")]
    [InlineData("DELETE", "/api/turno/-1")]
    public async Task EscrituraSinSesion_DebeExigirAutenticacion(
        string metodo,
        string ruta)
    {
        var direccion = Environment.GetEnvironmentVariable(
            "CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(
            Uri.TryCreate(direccion, UriKind.Absolute, out var url),
            "Se requiere una URL absoluta.");

        Assert.True(
            url!.IsLoopback && url.Scheme == "https",
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
                url.AbsoluteUri.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        using var solicitud = new HttpRequestMessage(
            new HttpMethod(metodo), ruta);

        // Solicitudes deliberadamente inválidas:
        // no crean registros ni modifican turnos existentes.
        if (metodo is "POST" or "PUT")
        {
            solicitud.Content = new StringContent(
                "{}",
                Encoding.UTF8,
                "application/json");
        }

        using var respuesta = await cliente.SendAsync(solicitud);

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
                    : new Uri(url, destino);

                bloqueado =
                    login.GetLeftPart(UriPartial.Authority) ==
                        url.GetLeftPart(UriPartial.Authority) &&
                    login.AbsolutePath.TrimEnd('/').Equals(
                        "/Account/Login",
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        Assert.True(
            bloqueado,
            $"{metodo} {ruta} sin sesión devolvió HTTP {codigo}. " +
            "Se esperaba 401, 403 o una redirección al login. " +
            "Un 400 o 404 no demuestra protección de autenticación.");
    }

    #endregion
}
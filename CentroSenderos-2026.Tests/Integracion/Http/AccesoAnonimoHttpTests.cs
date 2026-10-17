using System.Net;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Requiere el Server local ejecutándose. Solo realiza GET sobre un ID inexistente.
// No inicia la aplicación ni cambia la conexión del Server.
public class AccesoAnonimoHttpTests
{
    [Theory]
    [Trait("Categoria", "Http")]
    [Trait("Categoria", "Seguridad")]
    [InlineData("api/paciente/-1", "SEG-001")]
    [InlineData("api/profesional/-1", "SEG-002")]
    public async Task ConsultaSinSesion_DebeExigirAutenticacion(string ruta, string hallazgo)
    {
        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(Uri.TryCreate(direccion, UriKind.Absolute, out var baseUrl),
            "CENTROSENDEROS_TEST_URL debe contener una URL absoluta.");
        Assert.True(baseUrl!.IsLoopback && baseUrl.Scheme == "https",
            "Estas pruebas solo admiten un servidor local HTTPS.");

        // Sin cookies, credenciales ni redirecciones automáticas.
        using var handler = new HttpClientHandler
        {
            UseCookies = false,
            UseDefaultCredentials = false,
            AllowAutoRedirect = false
        };
        using var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl.AbsoluteUri.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await cliente.GetAsync(ruta);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "No se pudo conectar al Server local. Inicialo desde Visual Studio, " +
                "verificá el puerto y el certificado HTTPS. Esto es un bloqueo del entorno, " +
                "no una confirmación del defecto de acceso.");
        }

        using (respuesta)
        {
            var bloqueado = respuesta.StatusCode == HttpStatusCode.Unauthorized ||
                respuesta.StatusCode == HttpStatusCode.Forbidden;

            // También se admite una redirección explícita al login.
            var codigo = (int)respuesta.StatusCode;
            if (codigo is 301 or 302 or 303 or 307 or 308)
            {
                var destino = respuesta.Headers.Location;
                if (destino is not null)
                {
                    var login = destino.IsAbsoluteUri ? destino : new Uri(baseUrl, destino);
                    bloqueado = login.GetLeftPart(UriPartial.Authority) ==
                        baseUrl.GetLeftPart(UriPartial.Authority) &&
                        login.AbsolutePath.TrimEnd('/').Equals(
                            "/Account/Login", StringComparison.OrdinalIgnoreCase);
                }
            }

            Assert.True(bloqueado,
                $"{hallazgo}: GET /{ruta} sin sesión recibió HTTP {(int)respuesta.StatusCode}. " +
                "Se esperaba 401, 403 o una redirección al login. " +
                "Si devuelve el mensaje de registro inexistente, el controlador aceptó acceso anónimo.");
        }
    }
}

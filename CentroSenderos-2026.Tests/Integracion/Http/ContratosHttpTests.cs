using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Requiere Server local abierto y certificado HTTPS confiable.
// Los POST solo envían {}, sin datos válidos ni llamadas de edición o eliminación.
public class ContratosHttpTests
{
    [Theory]
    [Trait("Categoria", "Http")]
    [InlineData("api/tipoconsultorio/-1")]
    [InlineData("api/tipoobrasocial/-1")]
    [InlineData("api/tipoprestacion/-1")]
    [InlineData("api/socio/-1")]
    public async Task ConsultaInexistente_Devuelve404(string ruta)
    {
        using var cliente = CrearCliente();
        using var respuesta = await cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Theory]
    [Trait("Categoria", "Http")]
    [InlineData("api/paciente/insertar")]
    [InlineData("api/profesional")]
    [InlineData("api/tipoconsultorio")]
    [InlineData("api/tipoobrasocial")]
    [InlineData("api/tipoprestacion")]
    public async Task AltaConObjetoVacio_Devuelve400ConErroresDeValidacion(string ruta)
    {
        using var cliente = CrearCliente();
        using var contenido = new StringContent("{}", Encoding.UTF8, "application/json");
        using var respuesta = await cliente.PostAsync(ruta, contenido);
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.True(documento.RootElement.TryGetProperty("errors", out var errores),
            "Se esperaba la colección errors de la validación automática de ASP.NET Core.");
        Assert.Equal(JsonValueKind.Object, errores.ValueKind);
        Assert.True(errores.EnumerateObject().Any(),
            "El objeto vacío debe producir al menos un error de validación.");
    }

    private static HttpClient CrearCliente()
    {
        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var url) ||
            !url.IsLoopback || url.Scheme != "https")
            throw new InvalidOperationException("Se requiere una URL local HTTPS.");
        return new HttpClient(new HttpClientHandler
        {
            UseCookies = false,
            UseDefaultCredentials = false,
            AllowAutoRedirect = false
        })
        {
            BaseAddress = new Uri(url.AbsoluteUri.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
}

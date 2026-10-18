using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

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
        var config = new ConfigurationBuilder().AddUserSecrets<ContratosHttpTests>().Build();
        var email = config["Frontend:Email"];
        var password = config["Frontend:Password"];
        Assert.False(string.IsNullOrWhiteSpace(email), "Falta Frontend:Email.");
        Assert.False(string.IsNullOrWhiteSpace(password), "Falta Frontend:Password.");
        // Reutilizar la validación de URL local sin enviar solicitudes desde este cliente.
        using var cliente = CrearCliente();
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        await using var contexto = await navegador.NewContextAsync(new()
        {
            BaseURL = cliente.BaseAddress!.GetLeftPart(UriPartial.Authority)
        });
        var pagina = await contexto.NewPageAsync();
        await pagina.GotoAsync("/Account/Login", new() { WaitUntil = WaitUntilState.Load });
        var login = pagina.Locator("form").Filter(new() { Has = pagina.Locator("[id='Input.Email']") });
        await Assertions.Expect(login.Locator("input[name='__RequestVerificationToken']"))
            .ToHaveAttributeAsync("value", new Regex(".+"), new() { Timeout = 30000 });
        await pagina.Locator("[id='Input.Email']").FillAsync(email!);
        await pagina.Locator("[id='Input.Password']").FillAsync(password!);
        await login.EvaluateAsync("form => HTMLFormElement.prototype.submit.call(form)");
        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/turnos(?:[/?#]|$)"), new() { Timeout = 30000 });
        var respuesta = await pagina.APIRequest.PostAsync("/" + ruta.TrimStart('/'), new()
        {
            DataObject = new { },
            IgnoreHTTPSErrors = true
        });
        Assert.Equal(400, respuesta.Status);
        using var documento = JsonDocument.Parse(await respuesta.TextAsync());
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

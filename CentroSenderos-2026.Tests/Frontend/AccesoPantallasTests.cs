using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class AccesoPantallasTests
{
    [Theory]
    [Trait("Categoria", "Frontend")]
    [InlineData("/pacientes")]
    [InlineData("/profesionales")]
    public async Task ListadoSinSesion_MuestraInicioDeSesion(string ruta)
    {
        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var url) ||
            !url.IsLoopback || url.Scheme != "https")
            throw new InvalidOperationException("Se requiere una URL local HTTPS.");

        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });
        // Un contexto nuevo no comparte la sesión de tu navegador habitual.
        await using var contexto = await navegador.NewContextAsync(new()
        {
            BaseURL = url.GetLeftPart(UriPartial.Authority)
        });
        var pagina = await contexto.NewPageAsync();
        pagina.SetDefaultTimeout(15000);
        await pagina.GotoAsync(ruta);

        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/Account/Login(?:[/?#]|$)", RegexOptions.IgnoreCase));
        await Assertions.Expect(pagina.Locator("input[type='password']"))
            .ToBeVisibleAsync();
    }
}

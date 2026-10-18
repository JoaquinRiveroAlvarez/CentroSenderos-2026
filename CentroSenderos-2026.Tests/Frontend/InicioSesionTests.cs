using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class InicioSesionTests
{
    [Fact]
    [Trait("Categoria", "Frontend")]
    public async Task CredencialesValidas_PermitenAbrirPacientes()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<InicioSesionTests>().Build();
        var email = config["Frontend:Email"];
        var password = config["Frontend:Password"];
        Assert.False(string.IsNullOrWhiteSpace(email), "Falta Frontend:Email en secretos locales.");
        Assert.False(string.IsNullOrWhiteSpace(password), "Falta Frontend:Password en secretos locales.");

        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var url) ||
            !url.IsLoopback || url.Scheme != "https")
            throw new InvalidOperationException("Se requiere una URL local HTTPS.");

        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        await using var contexto = await navegador.NewContextAsync(new()
        {
            BaseURL = url.GetLeftPart(UriPartial.Authority)
        });
        var pagina = await contexto.NewPageAsync();
        pagina.SetDefaultTimeout(20000);
        await pagina.GotoAsync("/Account/Login");
        await pagina.Locator("[id='Input.Email']").FillAsync(email!);
        await pagina.Locator("[id='Input.Password']").FillAsync(password!);
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión", Exact = true })
            .ClickAsync();

        // El login actual redirige a Turnos al autenticar correctamente.
        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/turnos(?:[/?#]|$)", RegexOptions.IgnoreCase));
        await pagina.GotoAsync("/pacientes");
        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/pacientes(?:[/?#]|$)", RegexOptions.IgnoreCase));
        await Assertions.Expect(pagina.Locator(".pacientes-page")).ToBeVisibleAsync();
    }
}

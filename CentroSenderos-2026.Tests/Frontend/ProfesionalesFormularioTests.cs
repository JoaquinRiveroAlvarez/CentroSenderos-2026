using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ProfesionalesFormularioTests
{
    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task GuardarVacio_MuestraLosSieteCamposObligatorios()
        => ConFormulario(async pagina =>
        {
            await pagina.Locator(".profesional-form .save-button").ClickAsync();
            foreach (var mensaje in new[]
            {
                "Ingresá el nombre completo.",
                "Ingresá el área profesional.",
                "Ingresá el CUIT.",
                "Ingresá la Matrícula Profesional.",
                "Ingresá el RNP.",
                "Ingresá el teléfono.",
                "Ingresá el email."
            })
                await Assertions.Expect(pagina.Locator(".profesional-form")
                    .GetByText(mensaje, new() { Exact = true })).ToBeVisibleAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/profesionales/crear(?:[/?#]|$)"));
        });

    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task EmailConFormatoInvalido_MuestraValidacion()
        => ConFormulario(async pagina =>
        {
            await pagina.GetByPlaceholder("Ej: profesional@senderos.com", new() { Exact = true })
                .FillAsync("correo-invalido");
            // Los demás campos permanecen vacíos: no puede producirse un alta válida.
            await pagina.Locator(".profesional-form .save-button").ClickAsync();
            await Assertions.Expect(pagina.Locator(".profesional-form")
                .GetByText("El email no es válido.", new() { Exact = true })).ToBeVisibleAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/profesionales/crear(?:[/?#]|$)"));
        });

    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task Cancelar_ConNombreSinGuardar_RegresaAlListado()
        => ConFormulario(async pagina =>
        {
            await pagina.GetByPlaceholder("Insertar nombre completo", new() { Exact = true })
                .FillAsync("Prueba de cancelación");
            await pagina.Locator(".profesional-form .cancel-button").ClickAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/profesionales(?:[/?#]|$)"));
            await Assertions.Expect(pagina.Locator(".crear-profesional-page")).ToHaveCountAsync(0);
        });

    private static async Task ConFormulario(Func<IPage, Task> comprobar)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<ProfesionalesFormularioTests>().Build();
        var email = config["Frontend:Email"];
        var password = config["Frontend:Password"];
        Assert.False(string.IsNullOrWhiteSpace(email), "Falta Frontend:Email en secretos locales.");
        Assert.False(string.IsNullOrWhiteSpace(password), "Falta Frontend:Password en secretos locales.");

        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL") ?? "https://localhost:7069";
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var url) || !url.IsLoopback || url.Scheme != "https")
            throw new InvalidOperationException("Se requiere una URL local HTTPS.");

        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var navegador = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        await using var contexto = await navegador.NewContextAsync(new() { BaseURL = url.GetLeftPart(UriPartial.Authority) });
        var pagina = await contexto.NewPageAsync();
        pagina.SetDefaultTimeout(20000);
        await pagina.GotoAsync("/Account/Login", new() { WaitUntil = WaitUntilState.Load });
        // Esperar el formulario SSR completo y su token antes de enviar credenciales.
        var login = pagina.Locator("form").Filter(new() { Has = pagina.Locator("[id='Input.Email']") });
        await Assertions.Expect(login.Locator("input[name='__RequestVerificationToken']"))
            .ToHaveAttributeAsync("value", new Regex(".+"), new() { Timeout = 30000 });
        await pagina.Locator("[id='Input.Email']").FillAsync(email!);
        await pagina.Locator("[id='Input.Password']").FillAsync(password!);
        // Enviar el formulario HTML de Identity con sus campos ocultos y token.
        // Evita que la navegación mejorada intercepte este POST de autenticación.
        await login.EvaluateAsync("form => HTMLFormElement.prototype.submit.call(form)");
        await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/turnos(?:[/?#]|$)", RegexOptions.IgnoreCase), new() { Timeout = 30000 });
        await pagina.GotoAsync("/profesionales/crear");
        await Assertions.Expect(pagina.Locator(".profesional-form")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await comprobar(pagina);
    }
}

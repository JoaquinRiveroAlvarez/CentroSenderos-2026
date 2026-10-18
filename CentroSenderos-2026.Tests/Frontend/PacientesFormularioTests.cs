using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class PacientesFormularioTests
{
    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task GuardarVacio_MuestraLosSeisCamposObligatorios()
        => ConFormulario(async pagina =>
        {
            await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar Paciente", Exact = true }).ClickAsync();
            var formulario = pagina.Locator(".paciente-form");
            foreach (var mensaje in new[]
            {
                "El Nombre es obligatorio",
                "El DNI es obligatorio",
                "El Domicilio es obligatorio",
                "La fecha de nacimiento es obligatoria",
                "La Obra Social es obligatoria",
                "El Diagnóstico es obligatorio"
            })
                await Assertions.Expect(formulario.GetByText(mensaje, new() { Exact = true })).ToBeVisibleAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/pacientes/crear(?:[/?#]|$)"));
        });

    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task AgregarTelefono_ConNumeroVacio_MuestraAvisoYConservaUnaFila()
        => ConFormulario(async pagina =>
        {
            await pagina.Locator(".add-phone-button").ClickAsync();
            await Assertions.Expect(pagina.GetByText(
                "Completá el teléfono actual antes de agregar otro.", new() { Exact = true })).ToBeVisibleAsync();
            await Assertions.Expect(pagina.Locator(".phone-row")).ToHaveCountAsync(1);
        });

    [Fact]
    [Trait("Categoria", "Frontend")]
    public Task Cancelar_ConNombreSinGuardar_RegresaAlListado()
        => ConFormulario(async pagina =>
        {
            await pagina.GetByPlaceholder("Insertar nombre", new() { Exact = true }).FillAsync("Prueba de cancelación");
            await pagina.GetByRole(AriaRole.Button, new() { Name = "Cancelar", Exact = true }).ClickAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(new Regex(@"/pacientes(?:[/?#]|$)"));
            await Assertions.Expect(pagina.Locator(".pacientes-page")).ToBeVisibleAsync();
        });

    private static async Task ConFormulario(Func<IPage, Task> comprobar)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<PacientesFormularioTests>().Build();
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
        await pagina.GotoAsync("/pacientes/crear");
        await Assertions.Expect(pagina.Locator(".paciente-form")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await comprobar(pagina);
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ListadosBusquedaTests
{
    [Theory]
    [InlineData("/pacientes", "Buscar paciente...", ".paciente-nombre")]
    [InlineData("/profesionales", "Buscar profesional...", ".nombre-texto")]
    [Trait("Categoria", "Frontend")]
    public Task BuscarNombre_EnMayusculas_ConservaElRegistro(
        string ruta, string placeholder, string selectorNombre)
        => ConListado(ruta, async pagina =>
        {
            var nombre = (await pagina.Locator(selectorNombre).First.InnerTextAsync()).Trim();
            Assert.False(string.IsNullOrWhiteSpace(nombre), "Se requiere un registro con nombre para probar la búsqueda.");
            var buscador = pagina.GetByPlaceholder(placeholder, new() { Exact = true });
            // Confirmar primero una actualización observable del listado.
            await buscador.FillAsync("SINRESULTADO" + Guid.NewGuid().ToString("N"));
            await Assertions.Expect(pagina.GetByRole(AriaRole.Heading,
                new() { Name = "No encontramos resultados", Exact = true }))
                .ToBeVisibleAsync(new() { Timeout = 20000 });
            await buscador.FillAsync(nombre.ToUpperInvariant());
            await Assertions.Expect(pagina.Locator(selectorNombre)
                .Filter(new() { HasText = new Regex("^\\s*" + Regex.Escape(nombre) + "\\s*$") }).First)
                .ToBeVisibleAsync();
            var nombres = await pagina.Locator(selectorNombre).AllTextContentsAsync();
            Assert.All(nombres, actual => Assert.True(
                actual.Contains(nombre, StringComparison.OrdinalIgnoreCase),
                "La búsqueda por nombre muestra un nombre que no coincide."));
        });

    [Theory]
    [InlineData("/pacientes", "Buscar paciente...")]
    [InlineData("/profesionales", "Buscar profesional...")]
    [Trait("Categoria", "Frontend")]
    public Task BuscarSinCoincidencias_YLimpiar_RestauraElListado(string ruta, string placeholder)
        => ConListado(ruta, async pagina =>
        {
            var filas = pagina.Locator("table tbody tr");
            var cantidadInicial = await filas.CountAsync();
            var buscador = pagina.GetByPlaceholder(placeholder, new() { Exact = true });
            await buscador.FillAsync("SINRESULTADO" + Guid.NewGuid().ToString("N"));
            await Assertions.Expect(pagina.GetByRole(AriaRole.Heading,
                new() { Name = "No encontramos resultados", Exact = true }))
                .ToBeVisibleAsync(new() { Timeout = 20000 });
            await Assertions.Expect(filas).ToHaveCountAsync(0);
            await pagina.Locator(".estado-button").Filter(new() { HasText = "Limpiar búsqueda" }).ClickAsync();
            await Assertions.Expect(buscador).ToHaveValueAsync("");
            await Assertions.Expect(filas).ToHaveCountAsync(cantidadInicial);
        });

    private static async Task ConListado(string ruta, Func<IPage, Task> comprobar)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<ListadosBusquedaTests>().Build();
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
        await pagina.GotoAsync(ruta);
        var filas = pagina.Locator("table tbody tr");
        await Assertions.Expect(filas.First).ToBeVisibleAsync(new() { Timeout = 30000 });
        // Estas pruebas requieren al menos un registro existente; no crean datos.
        await comprobar(pagina);
    }
}

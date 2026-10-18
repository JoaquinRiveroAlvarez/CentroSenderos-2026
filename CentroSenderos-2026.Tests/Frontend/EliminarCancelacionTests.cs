using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class EliminarCancelacionTests
{
    [Theory]
    [InlineData("/pacientes", "Buscar paciente...", ".paciente-nombre", 1,
        "DNI", ".detalle-paciente-page", ".read-only-field")]
    [InlineData("/profesionales", "Buscar profesional...", ".nombre-texto", 5,
        "CUIT", ".detalle-profesional-page", ".form-value")]
    [Trait("Categoria", "Frontend")]
    public Task Eliminar_AbreConfirmacion_CancelarConservaRegistro(
        string ruta, string placeholder, string selectorNombre, int columnaDocumento,
        string etiquetaDocumento, string selectorDetalle, string selectorValor)
        => ConListado(ruta, async pagina =>
        {
            // Confirmar que el listado ya responde a eventos antes de seleccionar una fila.
            var buscador = pagina.GetByPlaceholder(placeholder, new() { Exact = true });
            await EscribirConsulta(buscador, "SINRESULTADO" + Guid.NewGuid().ToString("N"));
            await Assertions.Expect(pagina.GetByRole(AriaRole.Heading,
                new() { Name = "No encontramos resultados", Exact = true }))
                .ToBeVisibleAsync(new() { Timeout = 20000 });
            await pagina.Locator(".estado-button").Filter(new() { HasText = "Limpiar búsqueda" }).ClickAsync();
            await Assertions.Expect(buscador).ToHaveValueAsync("");

            var fila = pagina.Locator("table tbody tr").First;
            await Assertions.Expect(fila).ToBeVisibleAsync();
            var nombre = (await fila.Locator(selectorNombre).InnerTextAsync()).Trim();
            var documento = (await fila.Locator("td").Nth(columnaDocumento).InnerTextAsync()).Trim();
            Assert.False(string.IsNullOrWhiteSpace(nombre));
            Assert.False(string.IsNullOrWhiteSpace(documento));
            await fila.ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(Regex.Escape(ruta) + @"/detalle/[1-9][0-9]*(?:[/?#]|$)"),
                new() { Timeout = 30000 });
            var detalle = pagina.Locator(selectorDetalle);
            var campoNombre = detalle.Locator(".field-group").Filter(new()
            {
                Has = pagina.GetByText("Nombre completo", new() { Exact = true })
            }).Locator(selectorValor);
            var campoDocumento = detalle.Locator(".field-group").Filter(new()
            {
                Has = pagina.GetByText(etiquetaDocumento, new() { Exact = true })
            }).Locator(selectorValor);
            await Assertions.Expect(campoNombre).ToHaveTextAsync(nombre, new() { Timeout = 30000 });
            await Assertions.Expect(campoDocumento).ToHaveTextAsync(documento);
            var urlDetalle = pagina.Url;
            await detalle.Locator(".edit-button").ClickAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(Regex.Escape(ruta) + @"/editar/[1-9][0-9]*(?:[/?#]|$)"),
                new() { Timeout = 30000 });
            var formulario = pagina.Locator(ruta == "/pacientes" ? ".paciente-form" : ".profesional-form");
            var entradaNombre = formulario.GetByPlaceholder("Nombre completo", new() { Exact = true });
            var entradaDocumento = formulario.GetByPlaceholder(
                ruta == "/pacientes" ? "DNI" : "Ej: 20-12345678-9", new() { Exact = true });
            await Assertions.Expect(entradaNombre).ToHaveValueAsync(nombre, new() { Timeout = 30000 });
            await Assertions.Expect(entradaDocumento).ToHaveValueAsync(documento);
            var urlEdicion = pagina.Url;
            await formulario.Locator(".delete-button").ClickAsync();
            var modal = pagina.Locator(".delete-modal");
            await Assertions.Expect(modal).ToBeVisibleAsync();
            await Assertions.Expect(modal.Locator("h2")).ToHaveTextAsync(
                ruta == "/pacientes" ? "¿Eliminar paciente?" : "¿Eliminar profesional?");
            await Assertions.Expect(modal.Locator("strong")).ToHaveTextAsync(nombre);
            // Nunca se pulsa el botón que confirma la eliminación.
            await modal.Locator(".modal-cancel-button").ClickAsync();
            await Assertions.Expect(modal).ToHaveCountAsync(0);
            await Assertions.Expect(pagina).ToHaveURLAsync(urlEdicion);
            await Assertions.Expect(entradaNombre).ToHaveValueAsync(nombre);
            await formulario.Locator(".cancel-button").ClickAsync();
            await Assertions.Expect(pagina).ToHaveURLAsync(urlDetalle, new() { Timeout = 30000 });
            // Recargar obliga a consultar nuevamente el registro persistido.
            await pagina.ReloadAsync();
            await Assertions.Expect(campoNombre).ToHaveTextAsync(nombre, new() { Timeout = 30000 });
            await Assertions.Expect(campoDocumento).ToHaveTextAsync(documento);
        });

    private static async Task EscribirConsulta(ILocator buscador, string consulta)
    {
        // Reintentar solo si el reemplazo del DOM durante la carga borra lo escrito.
        // Las comprobaciones de filtrado se ejecutan una única vez, fuera de este método.
        for (var intento = 0; intento < 3; intento++)
        {
            await buscador.FillAsync("");
            await buscador.PressSequentiallyAsync(consulta, new() { Delay = 75 });
            await buscador.PressAsync("Tab");
            if (await buscador.InputValueAsync() == consulta)
                return;
        }
        await Assertions.Expect(buscador).ToHaveValueAsync(consulta);
    }

    private static async Task ConListado(string ruta, Func<IPage, Task> comprobar)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<EliminarCancelacionTests>().Build();
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

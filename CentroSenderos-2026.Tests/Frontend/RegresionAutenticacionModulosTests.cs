using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class RegresionAutenticacionModulosTests
{
    [Theory]
    [InlineData("turnos", false)]
    [InlineData("turnos", true)]
    [InlineData("socio", false)]
    [InlineData("socio", true)]
    [Trait("Categoria", "Frontend")]
    public async Task ConSesion_LosSelectoresSiguenCargando(string modulo, bool editar)
    {
        await using var db = BaseDePruebas.CrearContexto();
        var profesional = await db.Profesionales.AsNoTracking()
            .Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo).OrderBy(x => x.Id)
            .Select(x => x.Nombre).FirstAsync();
        var paciente = await db.Pacientes.AsNoTracking()
            .Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo).OrderBy(x => x.Id)
            .Select(x => x.Nombre).FirstAsync();
        var ruta = "/" + modulo + "/crear";
        if (editar)
        {
            var id = modulo == "turnos"
                ? await db.Turnos.Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo).Select(x => x.Id).FirstAsync()
                : await db.Socios.Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo && !x.EsCaja).Select(x => x.Id).FirstAsync();
            ruta = "/" + modulo + "/editar/" + id;
        }
        await ConListado(ruta, async pagina =>
        {
            // Verificar también que las mismas API aceptan una sesión de navegador válida.
            foreach (var api in new[] { "/api/paciente/ListaPaciente", "/api/profesional/ListaProfesional" })
            {
                var respuesta = await pagina.APIRequest.GetAsync(api);
                Assert.Equal(200, respuesta.Status);
            }

            if (modulo == "socio")
            {
                var selector = pagina.Locator(editar ? ".socio-form select.senderos-select" : "#socio-profesional");
                await Assertions.Expect(selector).ToBeVisibleAsync(new() { Timeout = 30000 });
                await Assertions.Expect(selector.Locator("option").Filter(new() { HasText = profesional }).First)
                    .ToHaveCountAsync(1, new() { Timeout = 30000 });
            }
            else
            {
                var botones = pagina.Locator(".selector-desplegable-boton");
                await Assertions.Expect(botones.First).ToBeVisibleAsync(new() { Timeout = 30000 });
                // Reintentar solo la apertura si la carga interactiva ignoró el primer clic.
                for (var intento = 0; intento < 5; intento++)
                {
                    await botones.First.ClickAsync();
                    try
                    {
                        await Assertions.Expect(pagina.GetByPlaceholder("Buscar profesional...", new() { Exact = true }))
                            .ToBeVisibleAsync(new() { Timeout = 2000 });
                        break;
                    }
                    catch (PlaywrightException) when (intento < 4) { }
                }
                await Assertions.Expect(pagina.Locator(".selector-desplegable-opcion")
                    .Filter(new() { HasText = profesional }).First).ToBeVisibleAsync();
                await botones.First.ClickAsync();
                await botones.Nth(1).ClickAsync();
                await Assertions.Expect(pagina.Locator(".selector-desplegable-opcion")
                    .Filter(new() { HasText = paciente }).First).ToBeVisibleAsync();
            }
            // Solo lectura: no guardar ni eliminar turnos o socios.
        });
    }

    private static async Task ConListado(string ruta, Func<IPage, Task> comprobar)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<RegresionAutenticacionModulosTests>().Build();
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

        await comprobar(pagina);
    }
}

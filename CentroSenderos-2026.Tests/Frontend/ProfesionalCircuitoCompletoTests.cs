using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Validaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Npgsql;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ProfesionalCircuitoCompletoTests
{
    [Fact]
    [Trait("Categoria", "Frontend")]
    public async Task AltaEdicionYBaja_ConProfesionalFicticio_ConservaDatos()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<ProfesionalCircuitoCompletoTests>().Build();
        var configServer = new ConfigurationBuilder()
            .AddUserSecrets("aspnet-CentroSenderos_2026_Server-ebda0453-8a02-44d5-a115-ed06d0d5bec7").Build();
        var testing = config.GetConnectionString("TestingConnection");
        var servidor = configServer.GetConnectionString("DefaultConnection");
        Assert.False(string.IsNullOrWhiteSpace(testing), "Falta TestingConnection.");
        Assert.False(string.IsNullOrWhiteSpace(servidor), "Falta DefaultConnection en secretos del Server.");
        var destinoTesting = new NpgsqlConnectionStringBuilder(testing);
        var destinoServer = new NpgsqlConnectionStringBuilder(servidor);
        Assert.True(string.Equals(destinoTesting.Host, destinoServer.Host, StringComparison.OrdinalIgnoreCase)
            && destinoTesting.Port == destinoServer.Port
            && destinoTesting.Database == destinoServer.Database
            && destinoTesting.Username == destinoServer.Username,
            "Los secretos del Server y Tests apuntan a destinos diferentes. Detener y revisar conexiones.");
        // CrearContexto también valida el host autorizado para testing.
        await using var db = BaseDePruebas.CrearContexto();

        string cuit;
        do { cuit = GenerarCuit(); }
        while (await db.Profesionales.AnyAsync(p => p.Cuit == cuit));
        var marca = Guid.NewGuid().ToString("N")[..12];
        var nombre = "Profesional Testing " + cuit;
        var editado = "Profesional Editado " + cuit;
        var mp = "TEST-MP-" + marca.ToUpperInvariant();
        var rnp = "TEST-RNP-" + marca.ToUpperInvariant();
        var correo = "testing." + marca + "@example.invalid";
        try
        {
            await ConListado("/profesionales/crear", async pagina =>
            {
                var entradaNombre = pagina.GetByPlaceholder("Insertar nombre completo", new() { Exact = true });
                // Confirmar interactividad mediante validación local.
                for (var intento = 0; intento < 5; intento++)
                {
                    await entradaNombre.FillAsync("Preparación");
                    await entradaNombre.PressAsync("Tab");
                    await entradaNombre.FillAsync("");
                    await entradaNombre.PressAsync("Tab");
                    try
                    {
                        await Assertions.Expect(pagina.GetByText("Ingresá el nombre completo.", new() { Exact = true }))
                            .ToBeVisibleAsync(new() { Timeout = 2000 });
                        break;
                    }
                    catch (PlaywrightException) when (intento < 4) { }
                }
                await EscribirConsulta(entradaNombre, nombre);
                await pagina.GetByPlaceholder("Ej: 20-12345678-9", new() { Exact = true }).FillAsync(cuit);
                await Assertions.Expect(pagina.Locator("#areaProfesional")).ToBeEnabledAsync(new() { Timeout = 30000 });
                var area = await pagina.Locator("#areaProfesional option").Nth(1).GetAttributeAsync("value");
                Assert.False(string.IsNullOrWhiteSpace(area), "Se requiere un área activa existente.");
                await pagina.Locator("#areaProfesional").SelectOptionAsync(area!);
                await pagina.GetByPlaceholder("Ingresar M.P.", new() { Exact = true }).FillAsync(mp);
                await pagina.GetByPlaceholder("Ingresar R.N.P.", new() { Exact = true }).FillAsync(rnp);
                await pagina.GetByPlaceholder("Ej: +54 351 1234567", new() { Exact = true }).FillAsync("3510000000");
                var email = pagina.GetByPlaceholder("Ej: profesional@senderos.com", new() { Exact = true });
                await email.FillAsync(correo);
                await email.PressAsync("Tab");
                // Rol y prestaciones quedan vacíos: no se asignan accesos ni se prueba FUN-001 aquí.
                await pagina.Locator(".profesional-form .save-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/profesionales/detalle/[1-9][0-9]*(?:[?#].*)?$"), new() { Timeout = 30000 });
                var campoNombre = pagina.Locator(".detalle-profesional-page .field-group")
                    .Filter(new() { Has = pagina.GetByText("Nombre completo", new() { Exact = true }) })
                    .Locator(".form-value");
                await Assertions.Expect(campoNombre).ToHaveTextAsync(nombre, new() { Timeout = 30000 });
                var creado = await db.Profesionales.AsNoTracking().SingleAsync(p => p.Cuit == cuit);
                Assert.Equal(nombre, creado.Nombre);
                Assert.Equal(EnumEstadoRegistro.activo, creado.EstadoRegistro);
                Assert.Equal(correo, creado.Email);
                Assert.Equal(area, creado.Area);
                Assert.Equal(mp, creado.MP);
                Assert.Equal(rnp, creado.RNP);

                await pagina.Locator(".detalle-profesional-page .edit-button").ClickAsync();
                var editarNombre = pagina.GetByPlaceholder("Nombre completo", new() { Exact = true });
                await Assertions.Expect(editarNombre).ToHaveValueAsync(nombre, new() { Timeout = 30000 });
                await EscribirConsulta(editarNombre, editado);
                await pagina.Locator(".profesional-form .save-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/profesionales/detalle/" + creado.Id + @"(?:[?#].*)?$"), new() { Timeout = 30000 });
                await Assertions.Expect(campoNombre).ToHaveTextAsync(editado, new() { Timeout = 30000 });
                var actualizado = await db.Profesionales.AsNoTracking().SingleAsync(p => p.Id == creado.Id);
                Assert.Equal(editado, actualizado.Nombre);
                Assert.Equal(cuit, actualizado.Cuit);
                Assert.Equal(mp, actualizado.MP);
                Assert.Equal(rnp, actualizado.RNP);

                await pagina.Locator(".detalle-profesional-page .edit-button").ClickAsync();
                await Assertions.Expect(editarNombre).ToHaveValueAsync(editado, new() { Timeout = 30000 });
                await pagina.Locator(".profesional-form .delete-button").ClickAsync();
                var modal = pagina.Locator(".delete-modal");
                await Assertions.Expect(modal.Locator("strong")).ToHaveTextAsync(editado);
                await modal.Locator(".confirm-delete-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/profesionales(?:[?#].*)?$"), new() { Timeout = 30000 });
                await Assertions.Expect(pagina.Locator(".success-message")
                    .GetByText("Profesional eliminado", new() { Exact = true })).ToBeVisibleAsync();
                var borrado = await db.Profesionales.AsNoTracking().SingleAsync(p => p.Id == creado.Id);
                Assert.Equal(EnumEstadoRegistro.borrado, borrado.EstadoRegistro);
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Uri(new Uri(pagina.Url), "/profesionales").AbsoluteUri, new() { Timeout = 30000 });
                await EscribirConsulta(pagina.GetByPlaceholder("Buscar profesional...", new() { Exact = true }), cuit);
                await Assertions.Expect(pagina.Locator("table tbody tr")).ToHaveCountAsync(0);
            });
        }
        finally
        {
            await using var limpieza = BaseDePruebas.CrearContexto();
            var propio = await limpieza.Profesionales.SingleOrDefaultAsync(p => p.Cuit == cuit
                && p.Email == correo && (p.Nombre == nombre || p.Nombre == editado));
            if (propio != null && propio.EstadoRegistro != EnumEstadoRegistro.borrado)
            {
                propio.EstadoRegistro = EnumEstadoRegistro.borrado;
                await limpieza.SaveChangesAsync();
            }
        }
    }

    private static string GenerarCuit()
    {
        // Identificador sintético con dígito verificador; no implica identidad real.
        while (true)
        {
            var baseCuit = "20" + Random.Shared.Next(80000000, 99000000);
            int[] pesos = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
            var suma = baseCuit.Select((digito, i) => (digito - '0') * pesos[i]).Sum();
            var verificador = 11 - suma % 11;
            if (verificador == 11) verificador = 0;
            if (verificador == 10) continue;
            var candidato = baseCuit + verificador;
            if (CuitValidador.EsValido(candidato)) return candidato;
        }
    }

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
        var config = new ConfigurationBuilder().AddUserSecrets<ProfesionalCircuitoCompletoTests>().Build();
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
        await Assertions.Expect(pagina.Locator(".profesional-form")).ToBeVisibleAsync(new() { Timeout = 30000 });
        await comprobar(pagina);
    }
}

using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Npgsql;
using System.Text.RegularExpressions;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class PacienteCircuitoCompletoTests
{
    [Fact]
    [Trait("Categoria", "Frontend")]
    public async Task AltaEdicionYBaja_ConPacienteFicticio_ConservaDatos()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<PacienteCircuitoCompletoTests>().Build();
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
        string dni;
        do { dni = Random.Shared.Next(80000000, 99000000).ToString(); }
        while (await db.Pacientes.AnyAsync(p => p.DNI == dni));
        var nombre = "Paciente Testing " + dni;
        var editado = "Paciente Editado " + dni;

        try
        {
            await ConListado("/pacientes/crear", async pagina =>
            {
                // Establecer interactividad mediante validación local antes de cargar datos.
                var entradaNombre = pagina.GetByPlaceholder("Insertar nombre", new() { Exact = true });
                var aviso = pagina.GetByText("El Nombre es obligatorio", new() { Exact = true });
                for (var intento = 0; intento < 5; intento++)
                {
                    await entradaNombre.FillAsync("Preparación");
                    await entradaNombre.PressAsync("Tab");
                    await entradaNombre.FillAsync("");
                    await entradaNombre.PressAsync("Tab");
                    try
                    {
                        await Assertions.Expect(aviso).ToBeVisibleAsync(new() { Timeout = 2000 });
                        break;
                    }
                    catch (PlaywrightException) when (intento < 4) { }
                }
                await EscribirConsulta(entradaNombre, nombre);
                await pagina.GetByPlaceholder("Insertar DNI", new() { Exact = true }).FillAsync(dni);
                await pagina.Locator("input[type=date]").FillAsync("2015-06-15");
                var obra = await pagina.Locator("#obraSocial option").Nth(1).GetAttributeAsync("value");
                var diagnostico = await pagina.Locator("#diagnostico option").Nth(1).GetAttributeAsync("value");
                Assert.False(string.IsNullOrWhiteSpace(obra), "Se requiere una obra social existente.");
                Assert.False(string.IsNullOrWhiteSpace(diagnostico), "Se requiere un diagnóstico existente.");
                await pagina.Locator("#obraSocial").SelectOptionAsync(obra!);
                await pagina.Locator("#diagnostico").SelectOptionAsync(diagnostico!);
                await pagina.GetByPlaceholder("Insertar teléfono", new() { Exact = true }).FillAsync("3510000000");
                await pagina.GetByPlaceholder("Insertar domicilio", new() { Exact = true }).FillAsync("Domicilio ficticio de testing");
                await pagina.GetByPlaceholder("Insertar domicilio", new() { Exact = true }).PressAsync("Tab");
                await pagina.Locator(".save-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/pacientes/detalle/[1-9][0-9]*(?:[/?#]|$)"), new() { Timeout = 30000 });
                var campoNombre = pagina.Locator(".detalle-paciente-page .field-group")
                    .Filter(new() { Has = pagina.GetByText("Nombre completo", new() { Exact = true }) })
                    .Locator(".read-only-field");
                await Assertions.Expect(campoNombre).ToHaveTextAsync(nombre, new() { Timeout = 30000 });
                var creado = await db.Pacientes.AsNoTracking().SingleAsync(p => p.DNI == dni);
                Assert.Equal(nombre, creado.Nombre);
                Assert.Equal(EnumEstadoRegistro.activo, creado.EstadoRegistro);
                Assert.Equal("Domicilio ficticio de testing", creado.Domicilio);
                var afiliado = creado.NumeroAfiliado;

                await pagina.Locator(".detalle-paciente-page .edit-button").ClickAsync();
                var editarNombre = pagina.GetByPlaceholder("Nombre completo", new() { Exact = true });
                await Assertions.Expect(editarNombre).ToHaveValueAsync(nombre, new() { Timeout = 30000 });
                await EscribirConsulta(editarNombre, editado);
                await pagina.Locator(".paciente-form .save-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/pacientes/detalle/" + creado.Id + @"(?:[/?#]|$)"), new() { Timeout = 30000 });
                await pagina.ReloadAsync();
                await Assertions.Expect(campoNombre).ToHaveTextAsync(editado, new() { Timeout = 30000 });
                var actualizado = await db.Pacientes.AsNoTracking().SingleAsync(p => p.Id == creado.Id);
                Assert.Equal(editado, actualizado.Nombre);
                Assert.Equal(dni, actualizado.DNI);
                Assert.Equal(afiliado, actualizado.NumeroAfiliado);

                await pagina.Locator(".detalle-paciente-page .edit-button").ClickAsync();
                await Assertions.Expect(editarNombre).ToHaveValueAsync(editado, new() { Timeout = 30000 });
                await pagina.Locator(".paciente-form .delete-button").ClickAsync();
                var modal = pagina.Locator(".delete-modal");
                await Assertions.Expect(modal.Locator("strong")).ToHaveTextAsync(editado);
                await modal.Locator(".confirm-delete-button").ClickAsync();
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/pacientes(?:[/?#]|$)"), new() { Timeout = 30000 });
                var borrado = await db.Pacientes.AsNoTracking().SingleAsync(p => p.Id == creado.Id);
                Assert.Equal(EnumEstadoRegistro.borrado, borrado.EstadoRegistro);
                var buscador = pagina.GetByPlaceholder("Buscar paciente...", new() { Exact = true });
                await EscribirConsulta(buscador, dni);
                await Assertions.Expect(pagina.Locator("table tbody tr")).ToHaveCountAsync(0);
            });
        }
        finally
        {
            // Si el circuito falla, dar de baja solo el registro ficticio de esta ejecución.
            // No se elimina físicamente ni se afectan registros ajenos.
            await using var limpieza = BaseDePruebas.CrearContexto();
            var propio = await limpieza.Pacientes.SingleOrDefaultAsync(p => p.DNI == dni
                && (p.Nombre == nombre || p.Nombre == editado));
            if (propio != null && propio.EstadoRegistro != EnumEstadoRegistro.borrado)
            {
                propio.EstadoRegistro = EnumEstadoRegistro.borrado;
                await limpieza.SaveChangesAsync();
            }
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
        var config = new ConfigurationBuilder().AddUserSecrets<PacienteCircuitoCompletoTests>().Build();
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
        await Assertions.Expect(pagina.Locator(".paciente-form")).ToBeVisibleAsync(new() { Timeout = 30000 });
        // Estas pruebas requieren al menos un registro existente; no crean datos.
        await comprobar(pagina);
    }
}

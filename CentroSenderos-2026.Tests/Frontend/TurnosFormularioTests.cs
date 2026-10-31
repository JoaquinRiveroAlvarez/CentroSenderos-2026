using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosFormularioTests
{
    #region Formulario de creación

    [Theory]
    [Trait("Categoria", "Frontend")]
    [InlineData("sinTipo")]
    [InlineData("sinConsultorio")]
    [InlineData("cancelar")]
    public async Task CrearTurno_ValidaDatosYPermiteCancelar(
        string escenario)
    {
        #region Configuración

        var direccion = Environment.GetEnvironmentVariable(
            "CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(
            Uri.TryCreate(direccion, UriKind.Absolute, out var url));

        Assert.True(
            url!.IsLoopback && url.Scheme == "https",
            "Se requiere un servidor local HTTPS.");

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<TurnosFormularioTests>()
            .Build();

        var email = configuracion["Frontend:Email"];
        var password = configuracion["Frontend:Password"];

        Assert.False(string.IsNullOrWhiteSpace(email));
        Assert.False(string.IsNullOrWhiteSpace(password));

        using var playwright = await Playwright.CreateAsync();

        await using var navegador =
            await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

        await using var contexto =
            await navegador.NewContextAsync(new()
            {
                BaseURL = url.GetLeftPart(UriPartial.Authority)
            });

        var pagina = await contexto.NewPageAsync();
        pagina.SetDefaultTimeout(30000);
        pagina.SetDefaultNavigationTimeout(60000);

        #endregion

        #region Inicio de sesión

        await pagina.GotoAsync("/Account/Login", new()
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 60000
        });

        var login = pagina.Locator("form").Filter(new()
        {
            Has = pagina.Locator("[id='Input.Email']")
        });

        await Assertions.Expect(
                login.Locator(
                    "input[name='__RequestVerificationToken']"))
            .ToHaveAttributeAsync(
                "value",
                new Regex(".+"),
                new() { Timeout = 30000 });

        await pagina.Locator("[id='Input.Email']")
            .FillAsync(email!);

        await pagina.Locator("[id='Input.Password']")
            .FillAsync(password!);

        await login.EvaluateAsync(
            "form => HTMLFormElement.prototype.submit.call(form)");

        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/turnos(?:[/?#]|$)"),
            new() { Timeout = 60000 });

        #endregion

        #region Apertura y comprobación de interactividad

        var fechaOrigen = DateTime.Today
            .AddDays(30)
            .ToString("yyyy-MM-dd");

        await pagina.GotoAsync(
            "/turnos/crear" +
            "?vistaOrigen=semanal" +
            $"&fechaOrigen={fechaOrigen}",
            new()
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 60000
            });

        var formulario = pagina.Locator(".turno-form");

        await Assertions.Expect(formulario)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        var calendario = pagina.Locator(".fecha-selector");

        for (var intento = 0; intento < 5; intento++)
        {
            await calendario.ClickAsync();

            try
            {
                await Assertions.Expect(
                        pagina.Locator(".calendario-crear"))
                    .ToBeVisibleAsync(new() { Timeout = 2000 });

                break;
            }
            catch (PlaywrightException) when (intento < 4)
            {
            }
        }

        await calendario.ClickAsync();

        await Assertions.Expect(
                pagina.Locator(".calendario-crear"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        #endregion

        #region Cancelación

        if (escenario == "cancelar")
        {
            await formulario.Locator(".btn-cancelar").ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(@"/turnos\?"),
                new() { Timeout = 30000 });

            var destino = new Uri(pagina.Url);

            Assert.Equal("/turnos", destino.AbsolutePath);
            Assert.Contains("vista=semanal", destino.Query);
            Assert.Contains($"fecha={fechaOrigen}", destino.Query);

            return;
        }

        #endregion

        #region Validaciones

        var tipo = formulario.Locator("#tipoTurno");
        var consultorio = formulario.Locator("#consultorio");

        await Assertions.Expect(tipo)
            .ToHaveValueAsync("0", new() { Timeout = 10000 });

        await Assertions.Expect(consultorio)
            .ToHaveValueAsync("0", new() { Timeout = 10000 });

        if (escenario == "sinConsultorio")
        {
            var opcion = tipo.Locator("option").Nth(1);

            await Assertions.Expect(opcion)
                .ToHaveCountAsync(1, new() { Timeout = 30000 });

            var valor = await opcion.GetAttributeAsync("value");

            Assert.False(string.IsNullOrWhiteSpace(valor));

            await tipo.SelectOptionAsync(valor!);

            await Assertions.Expect(tipo)
                .ToHaveValueAsync(valor!, new() { Timeout = 10000 });
        }

        var guardar = formulario.Locator(".btn-guardar");

        await Assertions.Expect(guardar)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        await Assertions.Expect(guardar)
            .ToBeEnabledAsync(new() { Timeout = 30000 });

        await guardar.ClickAsync();

        var mensajeEsperado = escenario == "sinTipo"
            ? "Seleccioná un tipo de turno."
            : "Seleccioná un consultorio.";

        await Assertions.Expect(
                pagina.Locator(".mensaje-form"))
            .ToHaveTextAsync(
                mensajeEsperado,
                new() { Timeout = 30000 });

        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/turnos/crear(?:[/?#]|$)"),
            new() { Timeout = 10000 });

        #endregion
    }

    #endregion
}
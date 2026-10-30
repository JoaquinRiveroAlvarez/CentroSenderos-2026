using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosConSesionHttpTests
{
    #region Consultas y validación con sesión

    [Theory]
    [Trait("Categoria", "Http")]
    [InlineData("GET", "/api/turno/ListaTurnos", 200)]
    [InlineData("GET", "/api/turno/-1", 404)]
    [InlineData("POST", "/api/turno", 400)]
    public async Task ConSesion_DevuelveLaRespuestaEsperada(
        string metodo,
        string ruta,
        int estadoEsperado)
    {
        #region Configuración

        var direccion = Environment.GetEnvironmentVariable(
            "CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(
            Uri.TryCreate(direccion, UriKind.Absolute, out var url),
            "Se requiere una URL absoluta.");

        Assert.True(
            url!.IsLoopback && url.Scheme == "https",
            "Estas pruebas solo admiten un servidor local HTTPS.");

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<TurnosConSesionHttpTests>()
            .Build();

        var email = configuracion["Frontend:Email"];
        var password = configuracion["Frontend:Password"];

        Assert.False(
            string.IsNullOrWhiteSpace(email),
            "Falta Frontend:Email en los secretos de Tests.");

        Assert.False(
            string.IsNullOrWhiteSpace(password),
            "Falta Frontend:Password en los secretos de Tests.");

        #endregion

        #region Navegador y login

        using var playwright = await Playwright.CreateAsync();

        await using var navegador =
            await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });

        await using var contexto =
            await navegador.NewContextAsync(new()
            {
                BaseURL = url.GetLeftPart(UriPartial.Authority)
            });

        var pagina = await contexto.NewPageAsync();

        await pagina.GotoAsync("/Account/Login", new()
        {
            WaitUntil = WaitUntilState.Load
        });

        var formulario = pagina.Locator("form").Filter(new()
        {
            Has = pagina.Locator("[id='Input.Email']")
        });

        await Assertions.Expect(
                formulario.Locator(
                    "input[name='__RequestVerificationToken']"))
            .ToHaveAttributeAsync(
                "value",
                new Regex(".+"),
                new() { Timeout = 30000 });

        await pagina.Locator("[id='Input.Email']")
            .FillAsync(email!);

        await pagina.Locator("[id='Input.Password']")
            .FillAsync(password!);

        await formulario.EvaluateAsync(
            "form => HTMLFormElement.prototype.submit.call(form)");

        await Assertions.Expect(pagina).ToHaveURLAsync(
            new Regex(@"/turnos(?:[/?#]|$)"),
            new() { Timeout = 30000 });

        #endregion

        #region Solicitud con las cookies de la sesión

        IAPIResponse respuesta;

        if (metodo == "POST")
        {
            // El objeto vacío debe rechazarse sin crear un turno.
            respuesta = await pagina.APIRequest.PostAsync(
                ruta,
                new()
                {
                    DataObject = new { },
                    IgnoreHTTPSErrors = true,
                    MaxRedirects = 0
                });
        }
        else
        {
            respuesta = await pagina.APIRequest.GetAsync(
                ruta,
                new()
                {
                    IgnoreHTTPSErrors = true,
                    MaxRedirects = 0
                });
        }

        try
        {
            Assert.Equal(estadoEsperado, respuesta.Status);

            using var documento = JsonDocument.Parse(
                await respuesta.TextAsync());

            if (estadoEsperado == 200)
            {
                Assert.Equal(
                    JsonValueKind.Array,
                    documento.RootElement.ValueKind);

                Assert.True(
                    documento.RootElement.GetArrayLength() > 0,
                    "Se esperaba el listado existente de turnos.");
            }

            if (metodo == "POST")
            {
                Assert.True(
                    documento.RootElement.TryGetProperty(
                        "errors", out var errores),
                    "Se esperaban errores de validación del DTO.");

                Assert.Equal(
                    JsonValueKind.Object,
                    errores.ValueKind);

                Assert.True(
                    errores.TryGetProperty("TipoTurnoId", out _));

                Assert.True(
                    errores.TryGetProperty("TipoConsultorioId", out _));
            }
        }
        finally
        {
            await respuesta.DisposeAsync();
        }

        #endregion
    }

    #endregion
}
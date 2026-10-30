using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosContratosAdicionalesTests
{
    #region Contratos HTTP con sesión

    [Theory]
    [Trait("Categoria", "Http")]
    [InlineData("eliminarInexistente", 404)]
    [InlineData("editarVacio", 400)]
    [InlineData("disponibilidadInvalida", 400)]
    public async Task SolicitudInvalida_DevuelveRespuestaControlada(
        string escenario,
        int estadoEsperado)
    {
        #region Configuración

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<TurnosContratosAdicionalesTests>()
            .Build();

        var email = configuracion["Frontend:Email"];
        var password = configuracion["Frontend:Password"];

        Assert.False(string.IsNullOrWhiteSpace(email));
        Assert.False(string.IsNullOrWhiteSpace(password));

        var direccion = Environment.GetEnvironmentVariable(
            "CENTROSENDEROS_TEST_URL")
            ?? "https://localhost:7069";

        Assert.True(
            Uri.TryCreate(direccion, UriKind.Absolute, out var url));

        Assert.True(
            url!.IsLoopback && url.Scheme == "https",
            "Se requiere un servidor local HTTPS.");

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
        pagina.SetDefaultTimeout(30000);
        pagina.SetDefaultNavigationTimeout(60000);

        #endregion

        #region Inicio de sesión

        await pagina.GotoAsync("/Account/Login");

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

        #region Solicitud

        IAPIResponse respuesta;

        switch (escenario)
        {
            case "eliminarInexistente":
                respuesta = await pagina.APIRequest.DeleteAsync(
                    "/api/turno/-1",
                    new()
                    {
                        IgnoreHTTPSErrors = true,
                        MaxRedirects = 0
                    });
                break;

            case "editarVacio":
                respuesta = await pagina.APIRequest.PutAsync(
                    "/api/turno/-1",
                    new()
                    {
                        DataObject = new { },
                        IgnoreHTTPSErrors = true,
                        MaxRedirects = 0
                    });
                break;

            default:
                var fecha = DateTime.Today
                    .AddDays(30)
                    .ToString("yyyy-MM-dd");

                respuesta = await pagina.APIRequest.GetAsync(
                    "/api/turno/Disponibles" +
                    $"?fecha={fecha}" +
                    "&tipoTurnoId=-1&consultorioId=-1",
                    new()
                    {
                        IgnoreHTTPSErrors = true,
                        MaxRedirects = 0
                    });
                break;
        }

        #endregion

        #region Comprobaciones

        try
        {
            Assert.Equal(estadoEsperado, respuesta.Status);

            using var documento = JsonDocument.Parse(
                await respuesta.TextAsync());

            var contenido = documento.RootElement;

            Assert.Equal(
                JsonValueKind.Object,
                contenido.ValueKind);

            if (escenario == "editarVacio")
            {
                Assert.True(
                    contenido.TryGetProperty(
                        "errors", out var errores));

                Assert.True(
                    errores.TryGetProperty(
                        "TipoTurnoId", out _));

                Assert.True(
                    errores.TryGetProperty(
                        "TipoConsultorioId", out _));
            }
            else
            {
                Assert.True(
                    contenido.TryGetProperty(
                        "mensaje", out var mensaje));

                var esperado = escenario == "eliminarInexistente"
                    ? "No encontramos el turno que querés eliminar."
                    : "El tipo de turno seleccionado no es válido.";

                Assert.Equal(esperado, mensaje.GetString());
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
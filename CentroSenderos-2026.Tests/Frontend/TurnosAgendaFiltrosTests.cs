using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosAgendaFiltrosTests
{
    #region Efecto de los filtros en la agenda

    [Theory]
    [Trait("Categoria", "Frontend")]
    [InlineData("consultorios")]
    [InlineData("tipos")]
    [InlineData("profesionales")]
    [InlineData("pacientes")]
    public async Task FiltroDesdeUrl_MuestraLosTurnosCorrespondientes(
        string filtro)
    {
        #region Resultado esperado desde el repositorio

        await using var db = BaseDePruebas.CrearContexto();

        var repositorio = new TurnoRepositorio(db);
        var lista = await repositorio.SelectListaTurnos();

        var referencia = lista
            .Where(t =>
                t.ProfesionalIds.Count > 0 &&
                t.PacienteIds.Count > 0)
            .OrderBy(t => t.FechaInicio)
            .FirstOrDefault();

        Assert.NotNull(referencia);

        var fecha = referencia.FechaInicio.Date;

        var valor = filtro switch
        {
            "consultorios" => referencia.TipoConsultorioId,
            "tipos" => referencia.TipoTurnoId,
            "profesionales" => referencia.ProfesionalIds.First(),
            "pacientes" => referencia.PacienteIds.First(),
            _ => throw new InvalidOperationException(
                "Filtro no previsto por la prueba.")
        };

        var esperados = lista
            .Where(t => t.FechaInicio.Date == fecha)
            .Where(t => filtro switch
            {
                "consultorios" => t.TipoConsultorioId == valor,
                "tipos" => t.TipoTurnoId == valor,
                "profesionales" => t.ProfesionalIds.Contains(valor),
                "pacientes" => t.PacienteIds.Contains(valor),
                _ => false
            })
            .ToList();

        Assert.NotEmpty(esperados);

        var horariosEsperados = esperados
            .Select(t =>
                $"{t.FechaInicio:HH:mm} - {t.FechaFin:HH:mm}")
            .OrderBy(texto => texto)
            .ToArray();

        #endregion

        #region Configuración e inicio de sesión

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<TurnosAgendaFiltrosTests>()
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

        #region Agenda filtrada

        await pagina.GotoAsync(
            $"/turnos?vista=diaria&fecha={fecha:yyyy-MM-dd}" +
            $"&{filtro}={valor}");

        var textoFecha = fecha.ToString(
            "d 'de' MMMM 'de' yyyy",
            new System.Globalization.CultureInfo("es-AR"));

        await Assertions.Expect(
                pagina.Locator(".fecha-actual"))
            .ToHaveTextAsync(
                textoFecha,
                new() { Timeout = 30000 });

        await Assertions.Expect(
                pagina.Locator(".agenda-principal .estado-pagina"))
            .ToHaveCountAsync(0, new() { Timeout = 30000 });

        await Assertions.Expect(
                pagina.Locator(".agenda-principal .mensaje-error"))
            .ToHaveCountAsync(0);

        // Confirmar que la página ya responde a eventos.
        var bloque = pagina.Locator(".filtro-bloque")
            .Nth(filtro switch
            {
                "consultorios" => 0,
                "tipos" => 1,
                "profesionales" => 2,
                "pacientes" => 3,
                _ => 0
            });

        var opciones = bloque.Locator(".filtro-opciones");

        for (var intento = 0; intento < 5; intento++)
        {
            if (await opciones.IsVisibleAsync())
            {
                break;
            }

            await bloque.Locator(".filtro-titulo").ClickAsync();

            try
            {
                await Assertions.Expect(opciones)
                    .ToBeVisibleAsync(new() { Timeout = 2000 });

                break;
            }
            catch (PlaywrightException) when (intento < 4)
            {
            }
        }

        await Assertions.Expect(opciones)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        await Assertions.Expect(
                pagina.Locator(".fecha-actual"))
            .ToHaveTextAsync(
                textoFecha,
                new() { Timeout = 30000 });

        #endregion

        #region Comparación de cantidad y horarios

        var tarjetas = pagina.Locator(
            ".agenda-principal .turno-card");

        await Assertions.Expect(tarjetas)
            .ToHaveCountAsync(
                esperados.Count,
                new() { Timeout = 30000 });

        var titulosHorarios = tarjetas.Locator(
            ".turno-horario-fila h2");

        // Esperar los títulos, además de sus tarjetas.
        await Assertions.Expect(titulosHorarios)
            .ToHaveCountAsync(
                esperados.Count,
                new() { Timeout = 30000 });

        // La agenda ordena sus turnos por hora de inicio.
        var horariosEnOrden = esperados
            .OrderBy(t => t.FechaInicio)
            .Select(t =>
                $"{t.FechaInicio:HH:mm} - {t.FechaFin:HH:mm}")
            .ToArray();

        // Esta aserción espera y reintenta hasta que
        // el contenido mostrado coincida.
        await Assertions.Expect(titulosHorarios)
            .ToHaveTextAsync(
                horariosEnOrden,
                new() { Timeout = 30000 });

        await Assertions.Expect(
                opciones.Locator("input[type='checkbox']:checked"))
            .ToHaveCountAsync(1, new() { Timeout = 30000 });
    }
        #endregion

    #endregion
}
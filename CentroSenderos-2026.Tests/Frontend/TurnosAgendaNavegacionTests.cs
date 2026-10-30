using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnosAgendaNavegacionTests
{
    #region Vistas y conservación de la navegación

    [Theory]
    [Trait("Categoria", "Frontend")]
    [InlineData("diaria")]
    [InlineData("ampliada")]
    [InlineData("semanal")]
    public async Task CrearYCancelar_ConservaVistaFechaYFiltros(
        string vista)
    {
        #region Datos de referencia

        await using var db = BaseDePruebas.CrearContexto();

        var turno = await db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EstadoRegistro == EnumEstadoRegistro.activo &&
                t.TurnoProfesionales.Any() &&
                t.TurnoPacientes.Any())
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.FechaInicio,
                t.TipoConsultorioId,
                t.TipoTurnoId,
                ProfesionalId = t.TurnoProfesionales
                    .Select(r => r.ProfesionalId)
                    .First(),
                PacienteId = t.TurnoPacientes
                    .Select(r => r.PacienteId)
                    .First()
            })
            .FirstOrDefaultAsync();

        Assert.NotNull(turno);

        var fechaSeleccionada = turno.FechaInicio
            .ToLocalTime()
            .Date;

        var fecha = fechaSeleccionada.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture);

        var filtros =
            $"&consultorios={turno.TipoConsultorioId}" +
            $"&tipos={turno.TipoTurnoId}" +
            $"&profesionales={turno.ProfesionalId}" +
            $"&pacientes={turno.PacienteId}";

        #endregion

        #region Configuración e inicio de sesión

        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<TurnosAgendaNavegacionTests>()
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

        #region Apertura de la agenda

        await pagina.GotoAsync(
            $"/turnos?vista={vista}&fecha={fecha}{filtros}",
            new()
            {
                WaitUntil = WaitUntilState.Load
            });

        await Assertions.Expect(
                pagina.Locator(".agenda-page"))
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        await ComprobarVista(
            pagina,
            vista,
            fechaSeleccionada);

        #endregion

        #region Confirmación de interactividad

        var filtroTipos = pagina.Locator(".filtro-bloque").Nth(1);
        var opcionesTipos = filtroTipos.Locator(".filtro-opciones");

        for (var intento = 0; intento < 5; intento++)
        {
            if (await opcionesTipos.IsVisibleAsync())
            {
                break;
            }

            await filtroTipos.Locator(".filtro-titulo").ClickAsync();

            try
            {
                await Assertions.Expect(opcionesTipos)
                    .ToBeVisibleAsync(new() { Timeout = 2000 });

                break;
            }
            catch (PlaywrightException) when (intento < 4)
            {
            }
        }

        await Assertions.Expect(opcionesTipos)
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        // Volver a comprobar el contexto una vez que
        // los controles de Blazor respondieron.
        await ComprobarVista(
            pagina,
            vista,
            fechaSeleccionada);

        #endregion

        #region Ir a Crear

        for (var intento = 0; intento < 5; intento++)
        {
            await pagina.Locator(".btn-nuevo-turno").ClickAsync();

            try
            {
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/turnos/crear\?"),
                    new() { Timeout = 3000 });

                break;
            }
            catch (PlaywrightException) when (intento < 4)
            {
            }
        }

        // La URL puede cambiar antes de terminar de renderizar
        // la página de destino. Esperar también el formulario.
        await Assertions.Expect(
                pagina.GetByRole(
                    AriaRole.Heading,
                    new()
                    {
                        Name = "Nuevo turno",
                        Exact = true
                    }))
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        await Assertions.Expect(
                pagina.Locator(".turno-form"))
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        var urlCrear = new Uri(pagina.Url);

        Assert.Equal("/turnos/crear", urlCrear.AbsolutePath);

        ComprobarParametro(urlCrear, "vistaOrigen", vista);
        ComprobarParametro(urlCrear, "fechaOrigen", fecha);

        ComprobarFiltros(
            urlCrear,
            turno.TipoConsultorioId,
            turno.TipoTurnoId,
            turno.ProfesionalId,
            turno.PacienteId);

        #endregion

        #region Cancelar y comprobar el regreso

        var cancelar = pagina.Locator(
            ".turno-form .btn-cancelar");

        for (var intento = 0; intento < 5; intento++)
        {
            await cancelar.ClickAsync();

            try
            {
                await Assertions.Expect(pagina).ToHaveURLAsync(
                    new Regex(@"/turnos\?"),
                    new() { Timeout = 3000 });

                break;
            }
            catch (PlaywrightException) when (intento < 4)
            {
            }
        }

        await Assertions.Expect(
                pagina.Locator(".agenda-page"))
            .ToBeVisibleAsync(new() { Timeout = 30000 });

        var regreso = new Uri(pagina.Url);

        Assert.Equal("/turnos", regreso.AbsolutePath);

        ComprobarParametro(regreso, "vista", vista);
        ComprobarParametro(regreso, "fecha", fecha);

        ComprobarFiltros(
            regreso,
            turno.TipoConsultorioId,
            turno.TipoTurnoId,
            turno.ProfesionalId,
            turno.PacienteId);

        await ComprobarVista(
            pagina,
            vista,
            fechaSeleccionada);

        #endregion
    }

    #endregion

    #region Comprobación visual de la agenda

    private static async Task ComprobarVista(
        IPage pagina,
        string vista,
        DateTime fecha)
    {
        await Assertions.Expect(
                pagina.Locator(".agenda-principal .estado-pagina"))
            .ToHaveCountAsync(0, new() { Timeout = 30000 });

        await Assertions.Expect(
                pagina.Locator(".agenda-principal .mensaje-error"))
            .ToHaveCountAsync(0);

        if (vista == "semanal")
        {
            await Assertions.Expect(
                    pagina.Locator(".agenda-semanal"))
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            await Assertions.Expect(
                    pagina.GetByRole(
                        AriaRole.Button,
                        new() { Name = "Semanal", Exact = true }))
                .ToHaveClassAsync(
                    new Regex("periodo-activo"),
                    new() { Timeout = 30000 });
        }
        else
        {
            var textoFecha = fecha.ToString(
                "d 'de' MMMM 'de' yyyy",
                new CultureInfo("es-AR"));

            await Assertions.Expect(
                    pagina.Locator(".fecha-actual"))
                .ToHaveTextAsync(
                    textoFecha,
                    new() { Timeout = 30000 });

            await Assertions.Expect(
                    pagina.GetByRole(
                        AriaRole.Button,
                        new() { Name = "Diaria", Exact = true }))
                .ToHaveClassAsync(
                    new Regex("periodo-activo"),
                    new() { Timeout = 30000 });

            if (vista == "ampliada")
            {
                await Assertions.Expect(
                        pagina.Locator(".agenda-ampliada"))
                    .ToBeVisibleAsync(new() { Timeout = 30000 });
            }
            else
            {
                await Assertions.Expect(
                        pagina.Locator(".agenda-ampliada"))
                    .ToHaveCountAsync(0);

                await Assertions.Expect(
                        pagina.Locator(".agenda-semanal"))
                    .ToHaveCountAsync(0);
            }
        }
    }

    #endregion

    #region Comprobación de parámetros

    private static void ComprobarFiltros(
        Uri url,
        int consultorio,
        int tipo,
        int profesional,
        int paciente)
    {
        ComprobarParametro(
            url, "consultorios", consultorio.ToString());

        ComprobarParametro(
            url, "tipos", tipo.ToString());

        ComprobarParametro(
            url, "profesionales", profesional.ToString());

        ComprobarParametro(
            url, "pacientes", paciente.ToString());
    }

    private static void ComprobarParametro(
        Uri url,
        string nombre,
        string esperado)
    {
        var valores = url.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(parte => parte.Split('=', 2))
            .Where(parte =>
                Uri.UnescapeDataString(parte[0]) == nombre)
            .Select(parte => parte.Length == 2
                ? Uri.UnescapeDataString(parte[1])
                : "")
            .ToList();

        Assert.Equal(
            esperado,
            Assert.Single(valores));
    }

    #endregion
}
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class TurnoAltaInterfazTests
{
    #region Circuito completo por interfaz

    [Fact]
    [Trait("Categoria", "Frontend")]
    public async Task CrearTurno_GuardaDatosYPermiteAbrirDetalle()
    {
        var tipoPruebaId = 0;

        try
        {
            #region Preparación de datos

            await using var db = BaseDePruebas.CrearContexto();

            var relacion = await db.ProfesionalTipoPrestaciones
                .Where(r =>
                    r.EstadoRegistro == EnumEstadoRegistro.activo &&
                    r.Profesional.EstadoRegistro ==
                        EnumEstadoRegistro.activo &&
                    r.TipoPrestacion.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                .Select(r => new
                {
                    r.ProfesionalId,
                    r.TipoPrestacionId,
                    NombreProfesional = r.Profesional.Nombre
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(relacion);

            var paciente = await db.Pacientes
                .Where(p =>
                    p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre
                })
                .FirstOrDefaultAsync();

            Assert.NotNull(paciente);

            var consultorioId = await db.Set<TipoConsultorio>()
                .Where(c =>
                    c.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            Assert.True(consultorioId > 0);

            var fecha = DateTime.UtcNow.Date.AddYears(2);
            var libre = false;

            for (var intento = 0; intento < 366; intento++)
            {
                var desde = DateTime.SpecifyKind(
                    fecha,
                    DateTimeKind.Utc);

                var hasta = desde.AddDays(1);

                if (!await db.Turnos.AnyAsync(t =>
                    t.FechaInicio < hasta &&
                    t.FechaFin > desde))
                {
                    libre = true;
                    break;
                }

                fecha = fecha.AddDays(1);
            }

            Assert.True(libre);

            var tipo = new TipoTurno
            {
                Tipo = $"TEST-UI-{Guid.NewGuid():N}",
                DuracionMinutos = 30,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            db.Add(tipo);
            await db.SaveChangesAsync();

            tipoPruebaId = tipo.Id;

            #endregion

            #region Configuración e inicio de sesión

            var configuracion = new ConfigurationBuilder()
                .AddUserSecrets<TurnoAltaInterfazTests>()
                .Build();

            var email = configuracion["Frontend:Email"];
            var password = configuracion["Frontend:Password"];

            Assert.False(string.IsNullOrWhiteSpace(email));
            Assert.False(string.IsNullOrWhiteSpace(password));

            var direccion = Environment.GetEnvironmentVariable(
                "CENTROSENDEROS_TEST_URL")
                ?? "https://localhost:7069";

            Assert.True(
                Uri.TryCreate(
                    direccion,
                    UriKind.Absolute,
                    out var url));

            Assert.True(
                url!.IsLoopback && url.Scheme == "https");

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

            #region Selección del profesional

            await pagina.GotoAsync(
                "/turnos/crear?vistaOrigen=diaria" +
                $"&fechaOrigen={fecha:yyyy-MM-dd}");

            await Assertions.Expect(
                    pagina.Locator(".turno-form"))
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            var botones = pagina.Locator(
                ".selector-desplegable-boton");

            for (var intento = 0; intento < 5; intento++)
            {
                await botones.First.ClickAsync();

                try
                {
                    await Assertions.Expect(
                            pagina.GetByPlaceholder(
                                "Buscar profesional...",
                                new() { Exact = true }))
                        .ToBeVisibleAsync(new() { Timeout = 2000 });

                    break;
                }
                catch (PlaywrightException) when (intento < 4)
                {
                }
            }

            await pagina.Locator(".selector-desplegable-opcion")
                .Filter(new()
                {
                    HasText = relacion.NombreProfesional
                })
                .First
                .ClickAsync();

            await pagina.GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Cerrar selectores",
                    Exact = true
                })
                .ClickAsync();

            #endregion

            #region Tipo, consultorio, prestación y paciente

            await pagina.Locator("#tipoTurno")
                .SelectOptionAsync(tipo.Id.ToString());

            await pagina.Locator("#consultorio")
                .SelectOptionAsync(consultorioId.ToString());

            var tarjetaProfesional = pagina.Locator(
                ".profesional-prestacion-card");

            await Assertions.Expect(tarjetaProfesional)
                .ToHaveCountAsync(1, new() { Timeout = 30000 });

            var selectorPrestacion =
                tarjetaProfesional.Locator("select");

            if (await selectorPrestacion.CountAsync() == 1)
            {
                await selectorPrestacion.SelectOptionAsync(
                    relacion.TipoPrestacionId.ToString());
            }
            else
            {
                await Assertions.Expect(
                        tarjetaProfesional.Locator(
                            ".prestacion-automatica"))
                    .ToBeVisibleAsync(new() { Timeout = 30000 });
            }

            await botones.Nth(1).ClickAsync();

            await pagina.Locator(".selector-desplegable-opcion")
                .Filter(new()
                {
                    HasText = paciente.Nombre
                })
                .First
                .ClickAsync();

            await pagina.GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "Cerrar selectores",
                    Exact = true
                })
                .ClickAsync();

            #endregion

            #region Horario y guardado

            var horario = pagina.Locator(".horario-opcion")
                .Filter(new()
                {
                    HasTextRegex = new Regex(@"^\s*08:00\s*$")
                });

            await Assertions.Expect(horario)
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            await horario.ClickAsync();

            await Assertions.Expect(
                    pagina.Locator(".horario-opcion.seleccionado"))
                .ToContainTextAsync("08:00");

            await pagina.Locator(".turno-form .btn-guardar")
                .ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(@"/turnos\?estado=creado"),
                new() { Timeout = 60000 });

            #endregion

            #region Verificación del alta en la base

            await using var verificacion =
                BaseDePruebas.CrearContexto();

            var guardados = await verificacion.Turnos
                .AsNoTracking()
                .Include(t => t.TurnoProfesionales)
                .Include(t => t.TurnoPacientes)
                .Include(t => t.TurnoTipoPrestaciones)
                .Where(t => t.TipoTurnoId == tipoPruebaId)
                .ToListAsync();

            var guardado = Assert.Single(guardados);

            var inicioEsperado = DateTime.SpecifyKind(
                fecha.AddHours(8),
                DateTimeKind.Utc);

            Assert.Equal(
                inicioEsperado,
                guardado.FechaInicio);

            Assert.Equal(
                inicioEsperado.AddMinutes(30),
                guardado.FechaFin);

            Assert.Equal(
                EnumEstadoTurno.reservado,
                guardado.EstadoTurno);

            Assert.Equal(
                consultorioId,
                guardado.TipoConsultorioId);

            Assert.Equal(
                relacion.ProfesionalId,
                Assert.Single(
                    guardado.TurnoProfesionales).ProfesionalId);

            Assert.Equal(
                paciente.Id,
                Assert.Single(
                    guardado.TurnoPacientes).PacienteId);

            var prestacionGuardada = Assert.Single(
                guardado.TurnoTipoPrestaciones);

            Assert.Equal(
                relacion.ProfesionalId,
                prestacionGuardada.ProfesionalId);

            Assert.Equal(
                relacion.TipoPrestacionId,
                prestacionGuardada.TipoPrestacionId);

            #endregion

            #region Detalle desde la agenda

            var tarjeta = pagina.Locator(".turno-card")
                .Filter(new()
                {
                    HasText = tipo.Tipo
                });

            await Assertions.Expect(tarjeta)
                .ToHaveCountAsync(1, new() { Timeout = 30000 });

            var detalle = pagina.GetByRole(
                AriaRole.Dialog,
                new()
                {
                    Name = "Detalle del turno",
                    Exact = true
                });

            for (var intento = 0; intento < 5; intento++)
            {
                await tarjeta.ClickAsync();

                try
                {
                    await Assertions.Expect(detalle)
                        .ToBeVisibleAsync(new() { Timeout = 3000 });

                    break;
                }
                catch (PlaywrightException) when (intento < 4)
                {
                    await Assertions.Expect(tarjeta)
                        .ToBeVisibleAsync(new() { Timeout = 10000 });
                }
            }

            await Assertions.Expect(detalle)
                .ToContainTextAsync(relacion.NombreProfesional);

            await Assertions.Expect(detalle)
                .ToContainTextAsync(paciente.Nombre);

            await Assertions.Expect(detalle)
                .ToContainTextAsync("08:00");

            await Assertions.Expect(detalle)
                .ToContainTextAsync("08:30");

            #endregion

            #region Edición desde el detalle

            await detalle.GetByRole(
                AriaRole.Button,
                new() { Name = "Editar turno" })
                .ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(
                    $@"/turnos/editar/{guardado.Id}(?:[/?#]|$)"),
                new() { Timeout = 30000 });

            var formularioEdicion = pagina.Locator(
                ".turno-editar-form");

            await Assertions.Expect(formularioEdicion)
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            var nuevoHorario = pagina.Locator(".horario-opcion")
                .Filter(new()
                {
                    HasTextRegex = new Regex(@"^\s*08:30\s*$")
                });

            await Assertions.Expect(nuevoHorario)
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            for (var intento = 0; intento < 5; intento++)
            {
                await nuevoHorario.ClickAsync();

                try
                {
                    await Assertions.Expect(
                            pagina.Locator(
                                ".horario-opcion.seleccionado"))
                        .ToContainTextAsync(
                            "08:30",
                            new() { Timeout = 2000 });

                    break;
                }
                catch (PlaywrightException) when (intento < 4)
                {
                }
            }

            await formularioEdicion.Locator(".btn-guardar")
                .ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(@"/turnos\?estado=actualizado"),
                new() { Timeout = 60000 });

            await using (var comprobarEdicion =
                BaseDePruebas.CrearContexto())
            {
                var editado = await comprobarEdicion.Turnos
                    .AsNoTracking()
                    .SingleAsync(t => t.Id == guardado.Id);

                Assert.Equal(
                    inicioEsperado.AddMinutes(30),
                    editado.FechaInicio);

                Assert.Equal(
                    inicioEsperado.AddMinutes(60),
                    editado.FechaFin);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    editado.EstadoRegistro);
            }

            #endregion

            #region Cancelación de la eliminación

            await pagina.GotoAsync(
                $"/turnos/editar/{guardado.Id}" +
                "?vistaOrigen=diaria" +
                $"&fechaOrigen={fecha:yyyy-MM-dd}");

            await Assertions.Expect(
                    pagina.Locator(".turno-editar-form"))
                .ToBeVisibleAsync(new() { Timeout = 30000 });

            var eliminar = pagina.Locator(".btn-eliminar");

            var confirmacion = pagina.Locator(".modal-eliminar")
                .Filter(new()
                {
                    HasText = "¿Eliminar este turno?"
                });

            for (var intento = 0; intento < 5; intento++)
            {
                await eliminar.ClickAsync();

                try
                {
                    await Assertions.Expect(confirmacion)
                        .ToBeVisibleAsync(new() { Timeout = 2000 });

                    break;
                }
                catch (PlaywrightException) when (intento < 4)
                {
                }
            }

            await confirmacion.GetByRole(
                AriaRole.Button,
                new()
                {
                    Name = "No, conservar",
                    Exact = true
                })
                .ClickAsync();

            await Assertions.Expect(confirmacion)
                .ToHaveCountAsync(0, new() { Timeout = 10000 });

            await using (var comprobarCancelacion =
                BaseDePruebas.CrearContexto())
            {
                var conservado = await comprobarCancelacion.Turnos
                    .AsNoTracking()
                    .SingleAsync(t => t.Id == guardado.Id);

                Assert.Equal(
                    EnumEstadoRegistro.activo,
                    conservado.EstadoRegistro);

                Assert.Equal(
                    inicioEsperado.AddMinutes(30),
                    conservado.FechaInicio);
            }

            #endregion

            #region Eliminación confirmada y baja lógica

            await eliminar.ClickAsync();

            await Assertions.Expect(confirmacion)
                .ToBeVisibleAsync(new() { Timeout = 10000 });

            await confirmacion.Locator(".btn-confirmar-eliminar")
                .ClickAsync();

            await Assertions.Expect(pagina).ToHaveURLAsync(
                new Regex(@"/turnos\?estado=eliminado"),
                new() { Timeout = 60000 });

            await using (var comprobarBaja =
                BaseDePruebas.CrearContexto())
            {
                var eliminado = await comprobarBaja.Turnos
                    .AsNoTracking()
                    .SingleAsync(t => t.Id == guardado.Id);

                Assert.Equal(
                    EnumEstadoRegistro.inactivo,
                    eliminado.EstadoRegistro);
            }

            await Assertions.Expect(
                    pagina.Locator(".turno-card")
                        .Filter(new()
                        {
                            HasText = tipo.Tipo
                        }))
                .ToHaveCountAsync(0, new() { Timeout = 30000 });

            #endregion
        }
        finally
        {
            #region Limpieza exclusiva de la prueba

            if (tipoPruebaId > 0)
            {
                await using var limpieza =
                    BaseDePruebas.CrearContexto();

                var ids = await limpieza.Turnos
                    .Where(t => t.TipoTurnoId == tipoPruebaId)
                    .Select(t => t.Id)
                    .ToListAsync();

                await limpieza.Set<TurnoTipoPrestacion>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Set<TurnoPaciente>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Set<TurnoProfesional>()
                    .Where(r => ids.Contains(r.TurnoId))
                    .ExecuteDeleteAsync();

                await limpieza.Turnos
                    .Where(t => t.TipoTurnoId == tipoPruebaId)
                    .ExecuteDeleteAsync();

                await limpieza.Set<TipoTurno>()
                    .Where(t => t.Id == tipoPruebaId)
                    .ExecuteDeleteAsync();
            }

            #endregion
        }
    }

    #endregion
}
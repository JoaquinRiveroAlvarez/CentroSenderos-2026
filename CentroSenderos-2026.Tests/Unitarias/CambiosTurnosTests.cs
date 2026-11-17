using System.Security.Claims;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CentroSenderos_2026.Tests.Unitarias;

// Base efímera: estos casos no usan Neon ni las credenciales del entorno.
public class CambiosTurnosTests
{
    private static ServiceProvider Servicios()
    {
        var servicios = new ServiceCollection();
        var conexion = new SqliteConnection("Data Source=:memory:");
        conexion.Open();
        servicios.AddSingleton(conexion);
        servicios.AddLogging();
        servicios.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(conexion));
        servicios.AddIdentityCore<MiUsuario>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        return servicios.BuildServiceProvider();
    }

    private static ProfesionalDTO ProfesionalNuevo() => new()
    {
        Nombre = "Ana Prueba", Area = "Salud", Cuit = "20123456786", MP = "", RNP = "RNP1",
        Telefono = "12345678", Email = "ana@example.test", TipoPrestacionIds = new() { 1, 2 },
        MatriculasPorPrestacion = new() { [1] = "PS-123", [2] = "TO-456" }
    };

    private static async Task Catalogos(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        db.TipoAreas.Add(new() { Tipo = "Salud", EstadoRegistro = EnumEstadoRegistro.activo });
        db.TipoPrestaciones.AddRange(
            new TipoPrestacion { Id = 1, Tipo = "Psicología", Cod = "PS", MontoSesion = 100, EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoPrestacion { Id = 2, Tipo = "Terapia", Cod = "TO", MontoSesion = 100, EstadoRegistro = EnumEstadoRegistro.activo });
        await db.SaveChangesAsync();
    }

    private static ProfesionalController Controlador(ApplicationDbContext db, UserManager<MiUsuario> usuarios) =>
        new(new ProfesionalRepositorio(db), db, usuarios)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "admin") }, "test")) } }
        };

    [Fact]
    public async Task Alta_CreaCuentaConPasswordYMatriculas_EditaSinDuplicarRelaciones()
    {
        using var servicios = Servicios();
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await Catalogos(db);
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole("profesional"));
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<MiUsuario>>();
        var dto = ProfesionalNuevo();
        var resultado = Assert.IsType<OkObjectResult>(await Controlador(db, usuarios).PostEvento(dto));
        var id = Assert.IsType<int>(resultado.Value);
        var usuario = await usuarios.FindByEmailAsync(dto.Email);
        Assert.NotNull(usuario);
        Assert.True(await usuarios.CheckPasswordAsync(usuario, "123456"));
        Assert.True(await usuarios.IsInRoleAsync(usuario, "profesional"));
        db.ChangeTracker.Clear();
        var repo = new ProfesionalRepositorio(db);
        var guardado = await repo.SelectPorId(id);
        Assert.NotNull(guardado);
        Assert.Equal("PS-123", guardado.MatriculasPorPrestacion[1]);
        Assert.Equal("TO-456", guardado.MatriculasPorPrestacion[2]);
        Assert.Equal("TO-456", (await repo.SelectListaProfesional()).Single().MatriculasPorPrestacion[2]);
        Assert.Equal("PS-123", (await repo.SelectByCuit(dto.Cuit))!.MatriculasPorPrestacion[1]);
        guardado.MatriculasPorPrestacion[1] = "PS-789";
        Assert.True(await repo.ActualizarProfesional(id, guardado));
        Assert.Equal(2, await db.ProfesionalTipoPrestaciones.CountAsync());
        Assert.Equal("PS-789", (await repo.SelectPorId(id))!.MatriculasPorPrestacion[1]);
    }

    [Fact]
    public async Task AltaInvalida_RevierteCuentaYProfesional()
    {
        using var servicios = Servicios();
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await Catalogos(db);
        await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync(new IdentityRole("profesional"));
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<MiUsuario>>();
        var dto = ProfesionalNuevo();
        dto.MatriculasPorPrestacion.Remove(2);
        Assert.IsType<BadRequestObjectResult>(await Controlador(db, usuarios).PostEvento(dto));
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Users.ToListAsync());
        Assert.Empty(await db.Profesionales.ToListAsync());
    }

    [Fact]
    public async Task AltaConEmailExistente_NoModificaLaCuenta()
    {
        using var servicios = Servicios();
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await Catalogos(db);
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<MiUsuario>>();
        var dto = ProfesionalNuevo();
        var usuario = new MiUsuario { UserName = dto.Email, Email = dto.Email };
        Assert.True((await usuarios.CreateAsync(usuario, "Original123!")).Succeeded);
        Assert.IsType<BadRequestObjectResult>(await Controlador(db, usuarios).PostEvento(dto));
        Assert.True(await usuarios.CheckPasswordAsync(usuario, "Original123!"));
        Assert.False(await usuarios.CheckPasswordAsync(usuario, "123456"));
        Assert.Empty(await db.Profesionales.ToListAsync());
    }

    [Fact]
    public async Task Horarios_IncluyenConflictoCompleto_LiberanCanceladoYExcluido()
    {
        using var servicios = Servicios();
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await Catalogos(db);
        var profesionalId = await new ProfesionalRepositorio(db).InsertarProfesional(ProfesionalNuevo());
        var consultorio = new TipoConsultorio { Tipo = "Sala 1" };
        var tipo = new TipoTurno { Tipo = "Individual", DuracionMinutos = 30 };
        var paciente = new Paciente { Nombre = "Paciente Prueba", DNI = "12345678", NumeroAfiliado = 1, Telefono = "123", Domicilio = "Calle", TipoObraSociales = new TipoObraSocial { Tipo = "Particular", Cuit = "20123456786" }, TipoDiagnosticos = new TipoDiagnostico { Tipo = "Consulta" } };
        var inicio = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);
        var turno = new Turno { FechaInicio = inicio, FechaFin = inicio.AddHours(1), TipoConsultorios = consultorio, TipoTurnos = tipo, EstadoRegistro = EnumEstadoRegistro.activo,
            TurnoProfesionales = new() { new() { ProfesionalId = profesionalId } },
            TurnoPacientes = new() { new() { Pacientes = paciente } } };
        db.Turnos.Add(turno);
        await db.SaveChangesAsync();
        var repo = new TurnoRepositorio(db);
        var fecha = DateOnly.FromDateTime(inicio);
        var horarios = await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id);
        Assert.False(horarios.Single(h => h.Hora == "09:30").Ocupado);
        Assert.False(horarios.Single(h => h.Hora == "11:00").Ocupado);
        var conflicto = Assert.Single(horarios.Single(h => h.Hora == "10:00").Conflictos);
        Assert.Equal("Sala 1", conflicto.Consultorio);
        Assert.True(conflicto.ConflictoConsultorio);
        Assert.Contains("Consultorio ocupado", conflicto.Motivos);
        Assert.Contains("Ana Prueba", conflicto.Profesionales);
        Assert.Contains("Paciente Prueba", conflicto.Pacientes);
        Assert.True((await repo.HorariosConOcupacion(fecha, tipo.Id, 0, profesionalIds: new() { profesionalId })).Single(h => h.Hora == "10:00").Ocupado);
        Assert.True((await repo.HorariosConOcupacion(fecha, tipo.Id, 0, pacienteIds: new() { paciente.Id })).Single(h => h.Hora == "10:00").Ocupado);
        Assert.All(await repo.HorariosConOcupacion(fecha, tipo.Id, 0), h => Assert.False(h.Ocupado));
        Assert.DoesNotContain(new TimeOnly(10, 0), await repo.HorariosDisponibles(fecha, tipo.Id, consultorio.Id));
        Assert.All(await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, turnoIdExcluir: turno.Id), h => Assert.False(h.Ocupado));
        var sugeridos = await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, intervaloMinutos: 30);
        Assert.Equal(24, sugeridos.Count);
        Assert.All(sugeridos, h => Assert.Equal(0, TimeOnly.Parse(h.Hora).Minute % 30));
        var libreExacto = Assert.Single(await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, hora: new TimeOnly(11, 7)));
        Assert.Equal("11:07", libreExacto.Hora);
        Assert.False(libreExacto.Ocupado);
        var ocupadoPorDuracion = Assert.Single(await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, hora: new TimeOnly(9, 47)));
        Assert.True(ocupadoPorDuracion.Ocupado);
        Assert.Equal("Sala 1", Assert.Single(ocupadoPorDuracion.Conflictos).Consultorio);
        Assert.False(Assert.Single(await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, turnoIdExcluir: turno.Id, hora: new TimeOnly(10, 17))).Ocupado);
        foreach (var invalida in new[] { new TimeOnly(7, 59), new TimeOnly(19, 45), new TimeOnly(10, 17, 30) })
            await Assert.ThrowsAsync<ApplicationException>(() => repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id, hora: invalida));
        turno.EstadoTurno = EnumEstadoTurno.cancelado;
        await db.SaveChangesAsync();
        Assert.All(await repo.HorariosConOcupacion(fecha, tipo.Id, consultorio.Id), h => Assert.False(h.Ocupado));
    }
}

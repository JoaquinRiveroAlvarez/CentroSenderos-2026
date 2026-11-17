using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_BD.Migrations;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Reglas;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CentroSenderos_2026.Tests.Unitarias;

// Todas las pruebas usan SQLite en memoria; no se conectan a Neon.
public class SesionIndividualTests
{
    [Theory]
    [InlineData("Sesión individual")]
    [InlineData(" SESION INDIVIDUAL ")]
    [InlineData("Sesión normal")]
    [InlineData("sesion normal")]
    public void IndividualExigeExactamenteUnPaciente_InclusoSiElCatalogoDiceOpcional(string nombre)
    {
        Assert.Equal(ReglasTurno.MensajePacienteIndividual, ReglasTurno.ErrorCantidadPacientes(nombre, false, 0));
        Assert.Null(ReglasTurno.ErrorCantidadPacientes(nombre, false, 1));
        Assert.Equal(ReglasTurno.MensajePacienteIndividual, ReglasTurno.ErrorCantidadPacientes(nombre, false, 2));
    }

    [Fact]
    public void OtrosTiposConservanSuRegla()
    {
        Assert.Null(ReglasTurno.ErrorCantidadPacientes("Sesión grupal", true, 2));
        Assert.Null(ReglasTurno.ErrorCantidadPacientes("Reunión", false, 0));
        Assert.NotNull(ReglasTurno.ErrorCantidadPacientes("Sesión grupal", true, 0));
    }

    private static async Task<ApplicationDbContext> CrearBase(SqliteConnection conexion)
    {
        await conexion.OpenAsync();
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.TipoTurnos.Add(new TipoTurno { Id = 1, Tipo = "Sesión individual", DuracionMinutos = 30, RequierePacientes = false });
        await db.SaveChangesAsync();
        return db;
    }

    private static TurnoDTO Turno(params int[] pacientes) => new()
    {
        Fecha = DateTime.Today.AddDays(1), Hora = new TimeOnly(10, 0),
        TipoTurnoId = 1, TipoConsultorioId = 1, ProfesionalIds = new() { 1 },
        PacienteIds = pacientes.ToList()
    };

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 2)]
    public async Task AltaYEdicionRechazanCantidadInvalidaAntesDePersistir(bool editar, int cantidad)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new TurnoRepositorio(db);
        var dto = Turno(Enumerable.Range(1, cantidad).ToArray());
        dto.ModificarTodaLaSerie = true;
        var error = await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            if (editar) await repo.ActualizarTurno(42, dto);
            else await repo.InsertarTurno(dto);
        });
        Assert.Equal(ReglasTurno.MensajePacienteIndividual, error.Message);
        Assert.Empty(await db.Turnos.ToListAsync());
        Assert.Empty(await db.Set<TurnoPaciente>().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnPacienteSeGuardaYSeEdita_TambienEnSerie(bool repetir)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        db.TipoConsultorios.Add(new() { Id = 1, Tipo = "Consultorio" });
        db.TipoPrestaciones.Add(new() { Id = 1, Tipo = "Psicología", Cod = "PS", MontoSesion = 100 });
        db.Profesionales.Add(new() { Id = 1, Nombre = "Profesional", Area = "Salud", Cuit = "20123456786", MP = "PS1", RNP = "RNP1", Telefono = "12345678", Email = "prof@example.test" });
        db.ProfesionalTipoPrestaciones.Add(new() { ProfesionalId = 1, TipoPrestacionId = 1 });
        db.TipoObraSociales.Add(new() { Id = 1, Tipo = "Particular" });
        db.TipoDiagnosticos.Add(new() { Id = 1, Tipo = "Consulta" });
        for (var i = 1; i <= 2; i++)
            db.Pacientes.Add(new() { Id = i, Nombre = $"Paciente {i}", DNI = $"1234567{i}", NumeroAfiliado = i, Telefono = "12345678", Domicilio = "Calle 1", TipoObraSocialId = 1, TipoDiagnosticoId = 1 });
        foreach (var entrada in db.ChangeTracker.Entries<CentroSenderos_2026_BD.Datos.EntityBase>().Where(e => e.State == EntityState.Added))
            entrada.Entity.EstadoRegistro = EnumEstadoRegistro.activo;
        await db.SaveChangesAsync();
        var repo = new TurnoRepositorio(db);
        var dto = Turno(1);
        dto.EstadoTurno = EnumEstadoTurno.reservado;
        dto.ProfesionalPrestaciones = new() { new() { ProfesionalId = 1, TipoPrestacionId = 1 } };
        if (repetir)
        {
            dto.FrecuenciaRecurrencia = EnumFrecuenciaRecurrenciaTurno.semanal;
            dto.FechaHastaRecurrencia = dto.Fecha.AddDays(7);
        }
        var id = await repo.InsertarTurno(dto);
        var cantidadEsperada = repetir ? 2 : 1;
        Assert.Equal(cantidadEsperada, await db.Turnos.CountAsync());
        Assert.All(await db.Set<TurnoPaciente>().ToListAsync(), p => Assert.Equal(1, p.PacienteId));
        dto = (await repo.SelectPorId(id))!;
        dto.ModificarTodaLaSerie = repetir;
        dto.PacienteIds = new() { 2 };
        Assert.True(await repo.ActualizarTurno(id, dto));
        db.ChangeTracker.Clear();
        Assert.Equal(cantidadEsperada, await db.Set<TurnoPaciente>().CountAsync());
        Assert.All(await db.Set<TurnoPaciente>().ToListAsync(), p => Assert.Equal(2, p.PacienteId));
        // Un intento inválido no cambia los pacientes ya guardados.
        dto.PacienteIds = new() { 1, 2 };
        await Assert.ThrowsAsync<ApplicationException>(() => repo.ActualizarTurno(id, dto));
        Assert.All(await db.Set<TurnoPaciente>().ToListAsync(), p => Assert.Equal(2, p.PacienteId));
    }

    [Fact]
    public async Task CatalogoNormalizaNombreAnteriorYNoPermitePacienteOpcional()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        db.TipoTurnos.RemoveRange(db.TipoTurnos);
        await db.SaveChangesAsync();
        var repo = new TipoTurnoRepositorio(db);
        var dto = new TipoTurnoDTO { Tipo = "Sesión normal", DuracionMinutos = 30, RequierePacientes = false };
        var id = await repo.InsertarTipoTurno(dto);
        var guardado = await repo.SelectPorId(id);
        Assert.Equal("Sesión individual", guardado!.Tipo);
        Assert.True(guardado.RequierePacientes);
        dto.Tipo = "SESION INDIVIDUAL";
        dto.RequierePacientes = false;
        Assert.True(await repo.ActualizarTipoTurno(id, dto));
        Assert.True((await repo.SelectPorId(id))!.RequierePacientes);
    }

    [Fact]
    public async Task MigracionRenombraElMismoRegistroSinCambiarLosOtrosTipos()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var tipo = await db.TipoTurnos.SingleAsync();
        tipo.Tipo = " SESIÓN NORMAL ";
        db.TipoTurnos.Add(new TipoTurno { Id = 2, Tipo = "Grupal", DuracionMinutos = 60, RequierePacientes = false });
        await db.SaveChangesAsync();
        // Equivalentes locales de las funciones PostgreSQL usadas por la migración de datos.
        conexion.CreateFunction<string, string>("btrim", s => s.Trim());
        conexion.CreateFunction<string, string, string, string>("translate", (s, origen, destino) =>
            new string(s.Select(c => origen.IndexOf(c) is var i && i >= 0 ? destino[i] : c).ToArray()));
        var migracion = new RenombrarSesionIndividual();
        Assert.Contains("20261117120000_RenombrarSesionIndividual", db.Database.GetMigrations());
        foreach (var operacion in migracion.UpOperations.OfType<SqlOperation>())
            await db.Database.ExecuteSqlRawAsync(operacion.Sql);
        db.ChangeTracker.Clear();
        var renombrado = await db.TipoTurnos.FindAsync(1);
        Assert.Equal("Sesión individual", renombrado!.Tipo);
        Assert.True(renombrado.RequierePacientes);
        Assert.False((await db.TipoTurnos.FindAsync(2))!.RequierePacientes);
        Assert.Equal(2, await db.TipoTurnos.CountAsync());
    }
}

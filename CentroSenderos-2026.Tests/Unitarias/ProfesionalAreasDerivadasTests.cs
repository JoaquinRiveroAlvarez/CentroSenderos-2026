using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026.Tests.Unitarias;

public class ProfesionalAreasDerivadasTests
{
    private static async Task<ApplicationDbContext> CrearBase(SqliteConnection conexion)
    {
        await conexion.OpenAsync();
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.TipoAreas.AddRange(
            new TipoArea { Id = 1, Tipo = "Salud", EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoArea { Id = 2, Tipo = "Educación", EstadoRegistro = EnumEstadoRegistro.activo });
        db.TipoPrestaciones.AddRange(
            new TipoPrestacion { Id = 1, Tipo = "Psicología", Cod = "PS", MontoSesion = 100, TipoAreaId = 1, EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoPrestacion { Id = 2, Tipo = "Fonoaudiología", Cod = "FO", MontoSesion = 100, TipoAreaId = 1, EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoPrestacion { Id = 3, Tipo = "Apoyo escolar", Cod = "AE", MontoSesion = 100, TipoAreaId = 2, EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoPrestacion { Id = 4, Tipo = "Sin clasificar", Cod = "SC", MontoSesion = 100, EstadoRegistro = EnumEstadoRegistro.activo });
        await db.SaveChangesAsync();
        return db;
    }

    private static ProfesionalDTO Profesional(params int[] prestaciones) => new()
    {
        Nombre = "Ana Prueba", Area = "Un área enviada por el cliente que debe ignorarse",
        Cuit = "20123456786", MP = "", RNP = "RNP1", Telefono = "12345678", Email = "ana@example.test",
        TipoPrestacionIds = prestaciones.ToList(),
        MatriculasPorPrestacion = prestaciones.ToDictionary(id => id, id => $"MP-{id}")
    };

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public async Task AreasSeDerivanYNoSeDuplicanEnTodasLasConsultas(int cantidadPrestaciones, int cantidadAreas)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new ProfesionalRepositorio(db);
        var id = await repo.InsertarProfesional(Profesional(Enumerable.Range(1, cantidadPrestaciones).ToArray()));
        var detalle = (await repo.SelectPorId(id))!;
        var porCuit = (await repo.SelectByCuit("20123456786"))!;
        var listado = (await repo.SelectListaProfesional()).Single();
        Assert.Equal(cantidadAreas, detalle.Areas.Count);
        Assert.Equal(detalle.Areas, porCuit.Areas);
        Assert.Equal(detalle.Areas, listado.Areas);
        Assert.DoesNotContain("enviada por el cliente", detalle.Area);
        Assert.Equal(cantidadPrestaciones, detalle.MatriculasPorPrestacion.Count);
        Assert.All(detalle.MatriculasPorPrestacion, matricula => Assert.Equal($"MP-{matricula.Key}", matricula.Value));
    }

    [Fact]
    public async Task CambiarPrestacionesActualizaAreasYConservaMatriculasRestantes()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new ProfesionalRepositorio(db);
        var id = await repo.InsertarProfesional(Profesional(1, 3));
        var dto = (await repo.SelectPorId(id))!;
        dto.TipoPrestacionIds = new() { 3 };
        dto.MatriculasPorPrestacion.Remove(1);
        Assert.True(await repo.ActualizarProfesional(id, dto));
        var guardado = (await repo.SelectPorId(id))!;
        Assert.Equal(new[] { "Educación" }, guardado.Areas);
        Assert.Equal("MP-3", guardado.MatriculasPorPrestacion[3]);
        Assert.Single(guardado.MatriculasPorPrestacion);
        (await db.TipoPrestaciones.FindAsync(3))!.TipoAreaId = 1;
        await db.SaveChangesAsync();
        Assert.Equal(new[] { "Salud" }, (await repo.SelectPorId(id))!.Areas);
    }

    [Fact]
    public async Task PendientesNoInventanAreasNiBorranElDatoLegado()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new ProfesionalRepositorio(db);
        var id = await repo.InsertarProfesional(Profesional(4));
        var entidad = (await db.Profesionales.FindAsync(id))!;
        entidad.Area = "Área histórica";
        await db.SaveChangesAsync();
        var dto = (await repo.SelectPorId(id))!;
        Assert.Empty(dto.Areas);
        Assert.Equal("Área histórica", dto.Area);
        dto.Nombre = "Ana Editada";
        Assert.True(await repo.ActualizarProfesional(id, dto));
        Assert.Equal("Área histórica", (await db.Profesionales.FindAsync(id))!.Area);
        Assert.Empty((await repo.SelectPorId(id))!.Areas);
        Assert.Equal("MP-4", (await repo.SelectPorId(id))!.MatriculasPorPrestacion[4]);
    }

    [Fact]
    public async Task RenombrarAreaSeReflejaSinEditarElProfesional()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new ProfesionalRepositorio(db);
        var id = await repo.InsertarProfesional(Profesional(1));
        (await db.TipoAreas.FindAsync(1))!.Tipo = "Salud integral";
        await db.SaveChangesAsync();
        Assert.Equal(new[] { "Salud integral" }, (await repo.SelectPorId(id))!.Areas);
    }
}

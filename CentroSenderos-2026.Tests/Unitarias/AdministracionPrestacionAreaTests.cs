using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026.Tests.Unitarias;

public class AdministracionPrestacionAreaTests
{
    private static async Task<ApplicationDbContext> CrearBase(SqliteConnection conexion)
    {
        await conexion.OpenAsync();
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.TipoAreas.AddRange(
            new TipoArea { Id = 1, Tipo = "Salud", EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoArea { Id = 2, Tipo = "Educación", EstadoRegistro = EnumEstadoRegistro.activo },
            new TipoArea { Id = 3, Tipo = "Inactiva", EstadoRegistro = EnumEstadoRegistro.inactivo },
            new TipoArea { Id = 4, Tipo = "Borrada", EstadoRegistro = EnumEstadoRegistro.borrado });
        await db.SaveChangesAsync();
        return db;
    }

    private static TipoPrestacionDTO Prestacion(int? area = null) => new()
    {
        Tipo = "Psicología", Cod = "PS", MontoSesion = 100, TipoAreaId = area
    };

    [Fact]
    public async Task AltaEdicionYConsultasConservanIdYNombreDeArea()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new TipoPrestacionRepositorio(db);
        var id = await repo.InsertarTipoPrestacion(Prestacion(1));
        Assert.Equal("Salud", (await repo.SelectPorId(id))!.TipoAreaNombre);
        Assert.Equal(1, (await repo.SelectByCod("PS"))!.TipoAreaId);
        Assert.Equal("Salud", (await repo.SelectListaTipoPrestacion()).Single().TipoAreaNombre);
        var dto = (await repo.SelectPorId(id))!;
        dto.TipoAreaId = 2;
        Assert.True(await repo.ActualizarTipoPrestacion(id, dto));
        Assert.Equal("Educación", (await repo.SelectPorId(id))!.TipoAreaNombre);
        (await db.TipoAreas.FindAsync(2))!.Tipo = "Educación y aprendizaje";
        await db.SaveChangesAsync();
        Assert.Equal("Educación y aprendizaje", (await repo.SelectListaTipoPrestacion()).Single().TipoAreaNombre);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task RechazaAreaInvalidaEnAltaYEdicionSinModificarLaPrestacion(int area)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new TipoPrestacionRepositorio(db);
        await Assert.ThrowsAsync<ApplicationException>(() => repo.InsertarTipoPrestacion(Prestacion(area)));
        Assert.Empty(await db.TipoPrestaciones.ToListAsync());
        var id = await repo.InsertarTipoPrestacion(Prestacion(1));
        var dto = Prestacion(area);
        dto.MontoSesion = 999;
        await Assert.ThrowsAsync<ApplicationException>(() => repo.ActualizarTipoPrestacion(id, dto));
        db.ChangeTracker.Clear();
        var guardado = (await repo.SelectPorId(id))!;
        Assert.Equal(1, guardado.TipoAreaId);
        Assert.Equal(100, guardado.MontoSesion);
    }

    [Fact]
    public async Task ContratosAnterioresPermitenAltaSinAreaYNoBorranAsignaciones()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new TipoPrestacionRepositorio(db);
        var id = await repo.InsertarTipoPrestacion(Prestacion());
        Assert.Null((await repo.SelectPorId(id))!.TipoAreaId);
        Assert.Null((await repo.SelectListaTipoPrestacion()).Single().TipoAreaNombre);
        Assert.True(await repo.ActualizarTipoPrestacion(id, Prestacion(1)));
        var anterior = Prestacion();
        anterior.MontoSesion = 150;
        Assert.True(await repo.ActualizarTipoPrestacion(id, anterior));
        var guardado = (await repo.SelectPorId(id))!;
        Assert.Equal(1, guardado.TipoAreaId);
        Assert.Equal(150, guardado.MontoSesion);
    }

    [Fact]
    public void ValidacionDeAreaNoReemplazaLaValidacionDeRetencion()
    {
        var dto = Prestacion(1);
        dto.PorcentajeRetencion = 101;
        var errores = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true));
        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(dto.PorcentajeRetencion)));
        Assert.DoesNotContain(errores, e => e.MemberNames.Contains(nameof(dto.TipoAreaId)));
    }
}

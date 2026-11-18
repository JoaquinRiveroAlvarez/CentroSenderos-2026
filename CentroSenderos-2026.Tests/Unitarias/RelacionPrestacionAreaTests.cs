using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CentroSenderos_2026.Tests.Unitarias;

// Esta etapa se prueba sin acceder a Neon ni a las cadenas de conexión del servidor.
public class RelacionPrestacionAreaTests
{
    private static async Task<ApplicationDbContext> CrearBase(SqliteConnection conexion)
    {
        await conexion.OpenAsync();
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static TipoPrestacionDTO Prestacion(string codigo = "PS") => new()
    {
        Tipo = "Psicología", Cod = codigo, MontoSesion = 100, PorcentajeRetencion = 20
    };

    [Fact]
    public async Task AltaYEdicionAnterioresSiguenFuncionandoSinArea()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var repo = new TipoPrestacionRepositorio(db);
        var dto = Prestacion();
        var id = await repo.InsertarTipoPrestacion(dto);
        Assert.Null((await db.TipoPrestaciones.FindAsync(id))!.TipoAreaId);
        dto.MontoSesion = 150;
        Assert.True(await repo.ActualizarTipoPrestacion(id, dto));
        Assert.Equal(150, (await repo.SelectPorId(id))!.MontoSesion);
        Assert.Null((await db.TipoPrestaciones.FindAsync(id))!.TipoAreaId);
    }

    [Fact]
    public async Task UnAreaPuedeTenerVariasPrestacionesYLaEdicionAnteriorNoBorraElVinculo()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        var area = new TipoArea { Tipo = "Salud", EstadoRegistro = EnumEstadoRegistro.activo };
        var repo = new TipoPrestacionRepositorio(db);
        var id = await repo.InsertarTipoPrestacion(Prestacion());
        var entidad = (await db.TipoPrestaciones.FindAsync(id))!;
        entidad.TipoArea = area;
        db.TipoPrestaciones.Add(new() { Tipo = "Fonoaudiología", Cod = "FO", MontoSesion = 100, TipoArea = area });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var dto = (await repo.SelectPorId(id))!;
        dto.MontoSesion = 200;
        Assert.True(await repo.ActualizarTipoPrestacion(id, dto));
        db.ChangeTracker.Clear();
        Assert.Equal(area.Id, (await db.TipoPrestaciones.FindAsync(id))!.TipoAreaId);
        Assert.Equal(2, (await db.TipoAreas.Include(a => a.TipoPrestaciones).SingleAsync()).TipoPrestaciones.Count);
    }

    [Fact]
    public async Task LaClaveForaneaRechazaUnAreaInexistente()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        db.TipoPrestaciones.Add(new() { Tipo = "Psicología", Cod = "PS", MontoSesion = 100, TipoAreaId = 999 });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task EliminarUnAreaReferenciadaNoBorraSusPrestaciones()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await using var db = await CrearBase(conexion);
        db.TipoPrestaciones.Add(new() { Tipo = "Psicología", Cod = "PS", MontoSesion = 100,
            TipoArea = new() { Tipo = "Salud", EstadoRegistro = EnumEstadoRegistro.activo }, EstadoRegistro = EnumEstadoRegistro.activo });
        await db.SaveChangesAsync();
        var id = (await db.TipoAreas.SingleAsync()).Id;
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<SqliteException>(() => db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"TipoAreas\" WHERE \"Id\" = {id}"));
        var error = await Assert.ThrowsAsync<ApplicationException>(() => new TipoAreaRepositorio(db).DeleteTipoArea(id));
        Assert.Contains("prestaciones", error.Message);
        Assert.Single(await db.TipoPrestaciones.ToListAsync());
        Assert.Equal(EnumEstadoRegistro.activo, (await db.TipoAreas.SingleAsync()).EstadoRegistro);
    }

    [Fact]
    public void ModeloYSnapshotCoincidenYLaMigracionSoloExpandeElEsquema()
    {
        // Obtener el modelo y generar SQL no abre conexiones.
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql("Host=localhost;Database=prueba_sin_conexion"));
        servicios.AddIdentityCore<MiUsuario>(o => o.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
            .AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        using var proveedor = servicios.BuildServiceProvider();
        using var db = proveedor.GetRequiredService<ApplicationDbContext>();
        Assert.False(db.Database.HasPendingModelChanges());
        var migraciones = db.Database.GetMigrations().ToList();
        var nueva = Assert.Single(migraciones.Where(m => m.EndsWith("_RelacionOpcionalPrestacionArea")));
        var ensamblado = db.GetService<IMigrationsAssembly>();
        var migracion = ensamblado.CreateMigration(ensamblado.Migrations[nueva], db.Database.ProviderName!);
        Assert.Collection(migracion.UpOperations,
            operacion =>
            {
                var columna = Assert.IsType<AddColumnOperation>(operacion);
                Assert.Equal("TipoAreaId", columna.Name);
                Assert.True(columna.IsNullable);
                Assert.Null(columna.DefaultValue);
                Assert.Null(columna.DefaultValueSql);
            },
            operacion => Assert.IsType<CreateIndexOperation>(operacion),
            operacion => Assert.Equal(ReferentialAction.Restrict, Assert.IsType<AddForeignKeyOperation>(operacion).OnDelete));
        var anterior = migraciones[migraciones.IndexOf(nueva) - 1];
        var sql = db.GetService<IMigrator>().GenerateScript(anterior, nueva);
        Assert.Contains("ADD \"TipoAreaId\" integer", sql);
        Assert.Contains("ON DELETE RESTRICT", sql);
        Assert.DoesNotContain("DROP COLUMN", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.DoesNotContain("AspNet", sql);
        Assert.DoesNotContain("UPDATE \"Profesionales\"", sql);
        Assert.DoesNotContain("UPDATE \"TipoPrestaciones\"", sql);
        Assert.Contains("\"Area\"", db.Database.GenerateCreateScript());
    }
}

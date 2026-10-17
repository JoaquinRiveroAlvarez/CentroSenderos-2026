using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace CentroSenderos_2026.Tests;

public class ObrasSocialesTests
{
    [Fact]
    public async Task AltaYConsulta_ConservaLosDatos()
    {
        await EnTransaccion(async (db, repo, dto) =>
        {
            var nombreEsperado = dto.Tipo;
            dto.Tipo = "  " + dto.Tipo.ToUpperInvariant() + "  ";
            var id = await repo.InsertarTipoObraSocial(dto);
            db.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(nombreEsperado, guardado.Tipo);
            Assert.Equal(dto.Cuit, guardado.Cuit);
            Assert.Equal(dto.Descripcion, guardado.Descripcion);
            Assert.Contains(await repo.SelectListaTipoObrasocial(), o => o.Id == id);
        });
    }
    [Fact]
    public async Task Edicion_PersisteDescripcion()
    {
        await EnTransaccion(async (db, repo, dto) =>
        {
            var id = await repo.InsertarTipoObraSocial(dto);
            dto.Descripcion = "Descripción actualizada";
            Assert.True(await repo.ActualizarTipoObraSocial(id, dto));
            db.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(dto.Descripcion, guardado.Descripcion);
            Assert.Equal(dto.Cuit, guardado.Cuit);
        });
    }
    [Fact]
    public async Task Baja_EsLogicaYExcluyeDelListado()
    {
        await EnTransaccion(async (db, repo, dto) =>
        {
            var id = await repo.InsertarTipoObraSocial(dto);
            Assert.True(await repo.DeleteTipoObraSocial(id));
            db.ChangeTracker.Clear();
            Assert.Equal(EnumEstadoRegistro.borrado,
                (await db.TipoObrasSociales.SingleAsync(o => o.Id == id)).EstadoRegistro);
            Assert.DoesNotContain(await repo.SelectListaTipoObrasocial(), o => o.Id == id);
        });
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Alta_RechazaNombreOCuitDuplicado(bool nombre)
    {
        await EnTransaccion(async (db, repo, dto) =>
        {
            await repo.InsertarTipoObraSocial(dto);
            var segundo = new TipoObraSocialDTO
            {
                Tipo = nombre ? dto.Tipo : $"Obra {Guid.NewGuid():N}",
                Cuit = nombre ? await CuitDisponible(db) : dto.Cuit,
                Descripcion = "Segundo registro",
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarTipoObraSocial(segundo); });
            Assert.Contains(nombre ? "nombre" : "CUIT", error.Message);
        });
    }
    [Fact]
    public async Task Alta_RechazaCuitInvalido()
    {
        await EnTransaccion(async (db, repo, dto) =>
        {
            dto.Cuit = "123";
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarTipoObraSocial(dto); });
            Assert.Equal("El CUIT ingresado no es válido.", error.Message);
        });
    }
    private static async Task EnTransaccion(
        Func<ApplicationDbContext, TipoObraSocialRepositorio, TipoObraSocialDTO, Task> prueba)
    {
        await using var db = BaseDePruebas.CrearContexto();
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var dto = new TipoObraSocialDTO
            {
                Tipo = $"Obra {DateTime.UtcNow.Ticks}{Random.Shared.Next(1000, 10000)}", Cuit = await CuitDisponible(db),
                Descripcion = "Datos de testing", EstadoRegistro = EnumEstadoRegistro.activo
            };
            await prueba(db, new TipoObraSocialRepositorio(db), dto);
        }
        finally { await tx.RollbackAsync(); }
    }
    private static async Task<string> CuitDisponible(ApplicationDbContext db)
    {
        int[] pesos = { 5,4,3,2,7,6,5,4,3,2 };
        for (int intento = 0; intento < 100; intento++)
        {
            var numero = "30" + Random.Shared.Next(10000000, 100000000);
            var suma = 0;
            for (int i = 0; i < 10; i++) suma += (numero[i] - '0') * pesos[i];
            var digito = 11 - suma % 11;
            if (digito == 10) continue;
            if (digito == 11) digito = 0;
            var cuit = numero + digito;
            if (!await db.TipoObrasSociales.AnyAsync(o => o.Cuit == cuit)) return cuit;
        }
        throw new InvalidOperationException("No se encontró un CUIT de prueba disponible.");
    }
}

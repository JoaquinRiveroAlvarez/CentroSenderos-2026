using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class PrestacionesTests
{
    [Fact]
    public async Task Alta_PermiteRecuperarLosDatos()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoPrestacion(datos);
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.True(id > 0);
            Assert.NotNull(guardado);
            Assert.Equal(datos.Tipo, guardado.Tipo);
            Assert.Equal(datos.Descripcion, guardado.Descripcion);
            Assert.Equal(datos.Cod, guardado.Cod); Assert.Equal(datos.MontoSesion, guardado.MontoSesion);
            Assert.Equal(EnumEstadoRegistro.activo, guardado.EstadoRegistro);
            Assert.Contains(await repo.SelectListaTipoPrestacion(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task Edicion_PersisteLosCambios()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoPrestacion(datos);
            datos.Descripcion = "Descripción actualizada";
            datos.MontoSesion = 9876.54m;
            Assert.True(await repo.ActualizarTipoPrestacion(id, datos));
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(datos.Descripcion, guardado.Descripcion);
            Assert.Equal(datos.Cod, guardado.Cod); Assert.Equal(datos.MontoSesion, guardado.MontoSesion);
        });
    }

    [Fact]
    public async Task Baja_EsLogicaYExcluyeDelListado()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoPrestacion(datos);
            Assert.True(await repo.DeleteTipoPrestacion(id));
            contexto.ChangeTracker.Clear();
            var entidad = await contexto.TipoPrestaciones.AsNoTracking().SingleAsync(p => p.Id == id);
            Assert.Equal(EnumEstadoRegistro.borrado, entidad.EstadoRegistro);
            Assert.DoesNotContain(await repo.SelectListaTipoPrestacion(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task Alta_RechazaCodDuplicado()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            await repo.InsertarTipoPrestacion(datos);
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarTipoPrestacion(datos); });
            Assert.Contains("Ya existe", error.Message);
            Assert.Contains(datos.Cod, error.Message);
        });
    }

    private static async Task EnTransaccion(
        Func<ApplicationDbContext, TipoPrestacionRepositorio, TipoPrestacionDTO, Task> prueba)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        try
        {
            var datos = new TipoPrestacionDTO
            {
                Tipo = $"Prueba {Guid.NewGuid():N}",
                Descripcion = "Registro de testing",
                Cod = $"TEST-{Guid.NewGuid():N}", MontoSesion = 1234.56m,
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            await prueba(contexto, new TipoPrestacionRepositorio(contexto), datos);
        }
        finally
        {
            await transaccion.RollbackAsync();
        }
    }
}

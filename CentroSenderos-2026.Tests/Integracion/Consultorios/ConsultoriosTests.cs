using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ConsultoriosTests
{
    [Fact]
    public async Task Alta_PermiteRecuperarLosDatos()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoConsultorio(datos);
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.True(id > 0);
            Assert.NotNull(guardado);
            Assert.Equal(datos.Tipo, guardado.Tipo);
            Assert.Equal(datos.Descripcion, guardado.Descripcion);
            Assert.Equal(datos.Direccion, guardado.Direccion);
            Assert.Equal(EnumEstadoRegistro.activo, guardado.EstadoRegistro);
            Assert.Contains(await repo.SelectListaTipoConsultorio(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task Edicion_PersisteLosCambios()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoConsultorio(datos);
            datos.Descripcion = "Descripción actualizada";
            datos.Direccion = "Calle Actualizada 456";
            Assert.True(await repo.ActualizarTipoConsultorio(id, datos));
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(datos.Descripcion, guardado.Descripcion);
            Assert.Equal(datos.Direccion, guardado.Direccion);
        });
    }

    [Fact]
    public async Task Baja_EsLogicaYExcluyeDelListado()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarTipoConsultorio(datos);
            Assert.True(await repo.DeleteTipoConsultorio(id));
            contexto.ChangeTracker.Clear();
            var entidad = await contexto.TipoConsultorios.AsNoTracking().SingleAsync(p => p.Id == id);
            Assert.Equal(EnumEstadoRegistro.borrado, entidad.EstadoRegistro);
            Assert.DoesNotContain(await repo.SelectListaTipoConsultorio(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task Alta_RechazaTipoDuplicado()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            await repo.InsertarTipoConsultorio(datos);
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarTipoConsultorio(datos); });
            Assert.Contains("Ya existe", error.Message);
            Assert.Contains(datos.Tipo, error.Message);
        });
    }

    private static async Task EnTransaccion(
        Func<ApplicationDbContext, TipoConsultorioRepositorio, TipoConsultorioDTO, Task> prueba)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        try
        {
            var datos = new TipoConsultorioDTO
            {
                Tipo = $"Prueba {Guid.NewGuid():N}",
                Descripcion = "Registro de testing",
                Direccion = "Calle Prueba 123",
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            await prueba(contexto, new TipoConsultorioRepositorio(contexto), datos);
        }
        finally
        {
            await transaccion.RollbackAsync();
        }
    }
}

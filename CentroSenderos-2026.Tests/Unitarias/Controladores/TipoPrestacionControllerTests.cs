using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Invocación directa con repositorio simulado: no prueba la canalización HTTP.
public class TipoPrestacionControllerTests
{
    [Fact]
    public async Task ConsultaExistente_Devuelve200YElDto()
    {
        var esperado = new TipoPrestacionDTO { Tipo = "Prueba", Cod = "TEST", MontoSesion = 1234.56m };
        var repo = RepositorioSimulado.Crear<ITipoPrestacionRepositorio>("SelectPorId",
            () => Task.FromResult<TipoPrestacionDTO?>(esperado));
        var respuesta = await new TipoPrestacionController(repo).GetById(123);
        var ok = Assert.IsType<OkObjectResult>(respuesta.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(esperado, ok.Value);
    }

    [Fact]
    public async Task ConsultaInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<ITipoPrestacionRepositorio>("SelectPorId",
            () => Task.FromResult<TipoPrestacionDTO?>(null));
        var respuesta = await new TipoPrestacionController(repo).GetById(-1);
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta.Result).StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task Alta_TraduceResultadoDelRepositorio(int esperado)
    {
        var repo = RepositorioSimulado.Crear<ITipoPrestacionRepositorio>("InsertarTipoPrestacion", () =>
        {
            if (esperado == 400) throw new ApplicationException("Regla rechazada");
            if (esperado == 500) throw new InvalidOperationException("Fallo simulado");
            return Task.FromResult(123);
        });
        var respuesta = await new TipoPrestacionController(repo).Post(new TipoPrestacionDTO { Tipo = "Prueba", Cod = "TEST", MontoSesion = 1234.56m });
        var resultado = Assert.IsAssignableFrom<ObjectResult>(respuesta);
        Assert.Equal(esperado, resultado.StatusCode);
        if (esperado == 200) Assert.Equal(123, Assert.IsType<int>(resultado.Value));
        if (esperado == 400)
        {
            Assert.NotNull(resultado.Value);
            Assert.Equal("Regla rechazada",
                resultado.Value.GetType().GetProperty("mensaje")!.GetValue(resultado.Value));
        }
    }

    [Fact]
    public async Task EdicionInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<ITipoPrestacionRepositorio>("ActualizarTipoPrestacion",
            () => Task.FromResult(false));
        var respuesta = await new TipoPrestacionController(repo).Put(-1, new TipoPrestacionDTO { Tipo = "Prueba", Cod = "TEST", MontoSesion = 1234.56m });
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta).StatusCode);
    }
}

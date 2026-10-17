using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Invocación directa con repositorio simulado: no prueba la canalización HTTP.
public class TipoObraSocialControllerTests
{
    [Fact]
    public async Task ConsultaExistente_Devuelve200YElDto()
    {
        var esperado = new TipoObraSocialDTO { Tipo = "Prueba" };
        var repo = RepositorioSimulado.Crear<ITipoObraSocialRepositorio>("SelectPorId",
            () => Task.FromResult<TipoObraSocialDTO?>(esperado));
        var respuesta = await new TipoObraSocialController(repo).GetById(123);
        var ok = Assert.IsType<OkObjectResult>(respuesta.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(esperado, ok.Value);
    }

    [Fact]
    public async Task ConsultaInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<ITipoObraSocialRepositorio>("SelectPorId",
            () => Task.FromResult<TipoObraSocialDTO?>(null));
        var respuesta = await new TipoObraSocialController(repo).GetById(-1);
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta.Result).StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task Alta_TraduceResultadoDelRepositorio(int esperado)
    {
        var repo = RepositorioSimulado.Crear<ITipoObraSocialRepositorio>("InsertarTipoObraSocial", () =>
        {
            if (esperado == 400) throw new ApplicationException("Regla rechazada");
            if (esperado == 500) throw new InvalidOperationException("Fallo simulado");
            return Task.FromResult(123);
        });
        var respuesta = await new TipoObraSocialController(repo).PostObraSocial(new TipoObraSocialDTO { Tipo = "Prueba", Cuit = "20987654326" });
        var resultado = Assert.IsAssignableFrom<ObjectResult>(respuesta.Result);
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
        var repo = RepositorioSimulado.Crear<ITipoObraSocialRepositorio>("ActualizarTipoObraSocial",
            () => Task.FromResult(false));
        var respuesta = await new TipoObraSocialController(repo).Put(-1, new TipoObraSocialDTO { Tipo = "Prueba", Cuit = "20987654326" });
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta).StatusCode);
    }
}

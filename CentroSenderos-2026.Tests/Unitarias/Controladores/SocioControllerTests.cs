using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Invocación directa con repositorio simulado: no prueba la canalización HTTP.
public class SocioControllerTests
{
    [Fact]
    public async Task ConsultaExistente_Devuelve200YElDto()
    {
        var esperado = new SocioListadoDTO { Id = 123, ProfesionalId = 456, Observacion = "Prueba" };
        var repo = RepositorioSimulado.Crear<ISocioRepositorio>("SelectPorId",
            () => Task.FromResult<SocioListadoDTO?>(esperado));
        var respuesta = await new SocioController(repo).GetSocioPorId(123);
        var ok = Assert.IsType<OkObjectResult>(respuesta.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(esperado, ok.Value);
    }

    [Fact]
    public async Task ConsultaInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<ISocioRepositorio>("SelectPorId",
            () => Task.FromResult<SocioListadoDTO?>(null));
        var respuesta = await new SocioController(repo).GetSocioPorId(-1);
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta.Result).StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task Alta_TraduceResultadoDelRepositorio(int esperado)
    {
        var repo = RepositorioSimulado.Crear<ISocioRepositorio>("InsertarSocio", () =>
        {
            if (esperado == 400) throw new ApplicationException("Regla rechazada");
            if (esperado == 500) throw new InvalidOperationException("Fallo simulado");
            return Task.FromResult(123);
        });
        var respuesta = await new SocioController(repo).InsertarSocio(new SocioDTO { ProfesionalId = 123, Observacion = "Prueba" });
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
        var repo = RepositorioSimulado.Crear<ISocioRepositorio>("ActualizarSocio",
            () => Task.FromResult(false));
        var respuesta = await new SocioController(repo).Put(-1, new SocioDTO { ProfesionalId = 123, Observacion = "Prueba" });
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(respuesta).StatusCode);
    }
}

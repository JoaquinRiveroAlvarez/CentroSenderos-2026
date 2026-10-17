using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Invocación directa: no comprueba middleware, autenticación ni validación HTTP automática.
public class PacienteControllerTests
{
    [Fact]
    public async Task ConsultaExistente_Devuelve200YElDto()
    {
        var esperado = new PacienteDTO { Nombre = "Prueba", DNI = "12345678" };
        var repo = RepositorioSimulado.Crear<IPacienteRepositorio>("SelectPorId",
            () => Task.FromResult<PacienteDTO?>(esperado));
        var controller = new PacienteController(repo);
        var resultado = await controller.GetPacientePorId(123);
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(esperado, ok.Value);
    }

    [Fact]
    public async Task ConsultaInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<IPacienteRepositorio>("SelectPorId",
            () => Task.FromResult<PacienteDTO?>(null));
        var resultado = await new PacienteController(repo).GetPacientePorId(-1);
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(resultado.Result).StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task Alta_TraduceResultadoDelRepositorio(int estadoEsperado)
    {
        var repo = RepositorioSimulado.Crear<IPacienteRepositorio>("InsertarPaciente", () =>
        {
            if (estadoEsperado == 400) throw new ApplicationException("Regla de negocio rechazada");
            if (estadoEsperado == 500) throw new InvalidOperationException("Fallo simulado");
            return Task.FromResult(123);
        });
        var controller = new PacienteController(repo);
        var respuesta = await controller.InsertarPaciente(new PacienteCrearDTO { Nombre = "Prueba", DNI = "12345678", Domicilio = "Calle Prueba" });
        var resultado = Assert.IsAssignableFrom<ObjectResult>(respuesta.Result);
        Assert.Equal(estadoEsperado, resultado.StatusCode);
        if (estadoEsperado == 200) Assert.Equal(123, Assert.IsType<int>(resultado.Value));
        if (estadoEsperado == 400)
        {
            Assert.NotNull(resultado.Value);
            var mensaje = resultado.Value.GetType().GetProperty("mensaje")!.GetValue(resultado.Value);
            Assert.Equal("Regla de negocio rechazada", mensaje);
        }
    }

    [Fact]
    public async Task EdicionInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<IPacienteRepositorio>("ActualizarPaciente",
            () => Task.FromResult(false));
        var resultado = await new PacienteController(repo).Put(-1, new PacienteDTO
        {
            Nombre = "Prueba", DNI = "12345678"
        });
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(resultado).StatusCode);
    }
}

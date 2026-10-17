using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Invocación directa: no comprueba middleware, autenticación ni validación HTTP automática.
public class ProfesionalControllerTests
{
    [Fact]
    public async Task ConsultaExistente_Devuelve200YElDto()
    {
        var esperado = new ProfesionalDTO { Nombre = "Prueba", Area = "Prueba", Cuit = "20987654326", MP = "MP-1", RNP = "RNP-1", Telefono = "3511234567", Email = "prueba@example.com" };
        var repo = RepositorioSimulado.Crear<IProfesionalRepositorio>("SelectPorId",
            () => Task.FromResult<ProfesionalDTO?>(esperado));
        var controller = new ProfesionalController(repo);
        var resultado = await controller.GetById(123);
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(esperado, ok.Value);
    }

    [Fact]
    public async Task ConsultaInexistente_Devuelve404()
    {
        var repo = RepositorioSimulado.Crear<IProfesionalRepositorio>("SelectPorId",
            () => Task.FromResult<ProfesionalDTO?>(null));
        var resultado = await new ProfesionalController(repo).GetById(-1);
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(resultado.Result).StatusCode);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task Alta_TraduceResultadoDelRepositorio(int estadoEsperado)
    {
        var repo = RepositorioSimulado.Crear<IProfesionalRepositorio>("InsertarProfesional", () =>
        {
            if (estadoEsperado == 400) throw new ApplicationException("Regla de negocio rechazada");
            if (estadoEsperado == 500) throw new InvalidOperationException("Fallo simulado");
            return Task.FromResult(123);
        });
        var controller = new ProfesionalController(repo);
        var respuesta = await controller.PostEvento(new ProfesionalDTO { Nombre = "Prueba", Area = "Prueba", Cuit = "20987654326", MP = "MP-1", RNP = "RNP-1", Telefono = "3511234567", Email = "prueba@example.com" });
        var resultado = Assert.IsAssignableFrom<ObjectResult>(respuesta);
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
        var repo = RepositorioSimulado.Crear<IProfesionalRepositorio>("ActualizarProfesional",
            () => Task.FromResult(false));
        var resultado = await new ProfesionalController(repo).Put(-1, new ProfesionalDTO
        {
            Nombre = "Prueba", Area = "Prueba", Cuit = "20987654326", MP = "MP-1", RNP = "RNP-1", Telefono = "3511234567", Email = "prueba@example.com"
        });
        Assert.Equal(404, Assert.IsType<NotFoundObjectResult>(resultado).StatusCode);
    }
}

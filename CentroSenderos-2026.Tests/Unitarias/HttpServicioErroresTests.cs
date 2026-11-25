using System.Net;
using CentroSenderos_2026_Servicio.ServiciosHttp;

namespace CentroSenderos_2026.Tests.Unitarias;

public class HttpServicioErroresTests
{
    [Theory]
    [InlineData("GET", 500)]
    [InlineData("POST", 500)]
    [InlineData("PUT", 500)]
    [InlineData("DELETE", 500)]
    [InlineData("MULTIPART", 500)]
    [InlineData("GET", 502)]
    [InlineData("GET", 503)]
    public async Task PaginaHtmlDeError_NoSeMuestraComoMensaje(string operacion, int estado)
    {
        using var cliente = Cliente((HttpStatusCode)estado, "  <!DOCTYPE html><html><body>Error del servidor</body></html>");
        var servicio = new HttpServicio(cliente);
        var respuesta = await Ejecutar(servicio, operacion);
        Assert.True(respuesta.Error);
        Assert.Null(respuesta.MensajeErrorServidor);
        Assert.Equal("El servidor no pudo completar la solicitud. Intentá nuevamente. Si continúa, avisá a administración.", respuesta.ObtenerError());
    }

    [Theory]
    [InlineData(401, "Iniciá sesión para continuar.")]
    [InlineData(403, "Tu cuenta no tiene permiso para realizar esta acción.")]
    [InlineData(404, "No hay datos disponibles.")]
    [InlineData(400, "Revisá los datos ingresados e intentá nuevamente.")]
    public async Task Html_ConservaElMensajeDelEstadoHttp(int estado, string mensaje)
    {
        using var cliente = Cliente((HttpStatusCode)estado, "<html><body>Página del servidor</body></html>");
        var respuesta = await new HttpServicio(cliente).Get<object>("api/profesional/ListaProfesional");
        Assert.True(respuesta.Error);
        Assert.Equal(mensaje, respuesta.ObtenerError());
    }

    [Fact]
    public async Task MensajeDeValidacion_SeConserva()
    {
        using var cliente = Cliente(HttpStatusCode.BadRequest, "Ya existe un profesional con ese CUIT.");
        var respuesta = await new HttpServicio(cliente).Post<object, object>("api/profesional", new { });
        Assert.Equal("Ya existe un profesional con ese CUIT.", respuesta.ObtenerError());
    }

    private static HttpClient Cliente(HttpStatusCode estado, string contenido) =>
        new(new RespuestaFija(estado, contenido)) { BaseAddress = new Uri("https://prueba.invalid/") };

    private static async Task<HttpRespuesta<object>> Ejecutar(HttpServicio servicio, string operacion)
    {
        using var multipart = new MultipartFormDataContent();
        return operacion switch
        {
            "GET" => await servicio.Get<object>("api/profesional/ListaProfesional"),
            "POST" => await servicio.Post<object, object>("api/profesional", new { }),
            "PUT" => await servicio.Put<object, object>("api/profesional/1", new { }),
            "DELETE" => await servicio.Delete("api/profesional/1"),
            _ => await servicio.PostMultipart<object>("api/documento", multipart)
        };
    }

    private sealed class RespuestaFija(HttpStatusCode estado, string contenido) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(contenido), RequestMessage = request });
    }
}

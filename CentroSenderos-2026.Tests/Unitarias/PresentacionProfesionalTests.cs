using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Presentacion;

namespace CentroSenderos_2026.Tests.Unitarias;

public class PresentacionProfesionalTests
{
    [Fact]
    public void ResumenLimpiaNombresYNoRepiteAreas()
    {
        var resultado = PresentacionProfesional.ObtenerResumenAreas(
            new[] { " Salud ", "salud", "", "Educación" });

        Assert.Equal("Educación, Salud", resultado);
    }

    [Fact]
    public void AreasPendientesNoSeReemplazanPorUnDatoInventado()
    {
        Assert.Equal("Sin área asignada", PresentacionProfesional.ObtenerResumenAreas(null));
        Assert.Equal("Sin área asignada", PresentacionProfesional.ObtenerResumenAreas(Array.Empty<string>()));
    }

    [Theory]
    [InlineData("lucia", true)]
    [InlineData(" EDUCACION ", true)]
    [InlineData("fonoaudiologia", true)]
    [InlineData("salud", true)]
    [InlineData("Área antigua", false)]
    [InlineData("Otra persona", false)]
    [InlineData("", true)]
    public void BusquedaUsaNombreAreasYPrestacionesSinDependerDelCampoAnterior(string busqueda, bool esperado)
    {
        var profesional = new ProfesionalListadoDTO
        {
            Nombre = "Lucía Álvarez",
            Area = "Área antigua",
            Areas = new() { "Educación", "Salud" },
            TipoPrestacionNombres = new() { "Fonoaudiología" }
        };

        Assert.Equal(esperado, PresentacionProfesional.CoincideBusqueda(profesional, busqueda));
    }
}

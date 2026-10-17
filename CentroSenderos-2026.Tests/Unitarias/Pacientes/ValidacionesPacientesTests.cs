using System.ComponentModel.DataAnnotations;
using CentroSenderos_2026_Shared.DTO;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ValidacionesPacientesTests
{
    [Theory]
    [InlineData(nameof(PacienteCrearDTO.Nombre))]
    [InlineData(nameof(PacienteCrearDTO.DNI))]
    [InlineData(nameof(PacienteCrearDTO.Domicilio))]
    public void CampoObligatorioVacio_ProduceErrorEnEseCampo(string campo)
    {
        var dto = CrearValido();
        typeof(PacienteCrearDTO).GetProperty(campo)!.SetValue(dto, "");
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Fact]
    public void FechaNacimientoAusente_ProduceError()
    {
        var dto = CrearValido();
        dto.FechaNacimiento = null;
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(nameof(dto.FechaNacimiento)));
    }

    [Theory]
    [InlineData(nameof(PacienteCrearDTO.TipoObraSocialId))]
    [InlineData(nameof(PacienteCrearDTO.TipoDiagnosticoId))]
    public void RelacionSinSeleccion_ProduceError(string campo)
    {
        var dto = CrearValido();
        typeof(PacienteCrearDTO).GetProperty(campo)!.SetValue(dto, 0);
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Theory]
    [InlineData(nameof(PacienteCrearDTO.Nombre), 51)]
    [InlineData(nameof(PacienteCrearDTO.DNI), 11)]
    [InlineData(nameof(PacienteCrearDTO.Domicilio), 31)]
    public void CampoExcedeLongitud_ProduceError(string campo, int longitud)
    {
        var dto = CrearValido();
        typeof(PacienteCrearDTO).GetProperty(campo)!.SetValue(dto, new string('1', longitud));
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Fact]
    public void DatosValidos_NoProducenErrores()
    {
        Assert.Empty(Validar(CrearValido()));
    }

    // Los IDs positivos solo satisfacen los atributos del DTO.
    // La existencia real de las relaciones se prueba en integración.
    private static PacienteCrearDTO CrearValido() => new()
    {
        Nombre = "Paciente Prueba", DNI = "12345678",
        Domicilio = "Calle Prueba 123",
        FechaNacimiento = new DateTime(2000, 1, 15),
        TipoObraSocialId = 1, TipoDiagnosticoId = 1,
        Telefonos = new()
        {
            new PacienteTelefonoDTO { Numero = "3511234567", Etiqueta = "Personal" }
        }
    };

    private static List<ValidationResult> Validar(object dto)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true);
        return errores;
    }
}

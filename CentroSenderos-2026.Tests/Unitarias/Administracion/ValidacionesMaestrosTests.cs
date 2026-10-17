using System.ComponentModel.DataAnnotations;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Valida atributos de los DTO; no sustituye pruebas HTTP ni restricciones de base de datos.
public class ValidacionesMaestrosTests
{
    [Theory]
    [InlineData("Consultorio")]
    [InlineData("ObraSocial")]
    [InlineData("Prestacion")]
    public void DatosValidos_NoProducenErrores(string entidad)
    {
        Assert.Empty(Validar(CrearValido(entidad)));
    }

    [Theory]
    [InlineData("Consultorio", "Tipo")]
    [InlineData("ObraSocial", "Tipo")]
    [InlineData("ObraSocial", "Cuit")]
    [InlineData("Prestacion", "Tipo")]
    [InlineData("Prestacion", "Cod")]
    public void CampoObligatorioVacio_ProduceErrorEnElCampo(string entidad, string campo)
    {
        var dto = CrearValido(entidad);
        dto.GetType().GetProperty(campo)!.SetValue(dto, "");
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Theory]
    [InlineData("Consultorio", "Tipo", 101)]
    [InlineData("Consultorio", "Descripcion", 201)]
    [InlineData("Consultorio", "Direccion", 101)]
    [InlineData("ObraSocial", "Tipo", 51)]
    [InlineData("ObraSocial", "Descripcion", 51)]
    [InlineData("Prestacion", "Tipo", 51)]
    [InlineData("Prestacion", "Descripcion", 51)]
    [InlineData("Prestacion", "Cod", 101)]
    public void CampoExcedeLongitud_ProduceErrorEnElCampo(string entidad, string campo, int longitud)
    {
        var dto = CrearValido(entidad);
        dto.GetType().GetProperty(campo)!.SetValue(dto, new string('A', longitud));
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Fact]
    public void ObraSocial_CuitConDigitoIncorrecto_ProduceError()
    {
        var dto = (TipoObraSocialDTO)CrearValido("ObraSocial");
        dto.Cuit = "20987654323";
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(nameof(dto.Cuit)));
    }

    private static object CrearValido(string entidad) => entidad switch
    {
        "Consultorio" => new TipoConsultorioDTO
        {
            Tipo = "Consultorio Prueba", Descripcion = "Datos de prueba",
            Direccion = "Calle Prueba 123", EstadoRegistro = EnumEstadoRegistro.activo
        },
        "ObraSocial" => new TipoObraSocialDTO
        {
            Tipo = "Obra Prueba", Descripcion = "Datos de prueba",
            Cuit = "20987654326", EstadoRegistro = EnumEstadoRegistro.activo
        },
        "Prestacion" => new TipoPrestacionDTO
        {
            Tipo = "Prestación Prueba", Descripcion = "Datos de prueba",
            Cod = "PRUEBA-01", MontoSesion = 1234.56m,
            EstadoRegistro = EnumEstadoRegistro.activo
        },
        _ => throw new ArgumentException("Entidad de prueba desconocida.", nameof(entidad))
    };

    private static List<ValidationResult> Validar(object dto)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true);
        return errores;
    }
}

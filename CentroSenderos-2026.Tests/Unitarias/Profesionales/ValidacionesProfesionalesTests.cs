using System.ComponentModel.DataAnnotations;
using CentroSenderos_2026_Shared.DTO;
using Xunit;

namespace CentroSenderos_2026.Tests;

// Comprueba los atributos del DTO, no el renderizado del formulario ni la API.
public class ValidacionesProfesionalesTests
{
    [Theory]
    [InlineData(nameof(ProfesionalDTO.Nombre))]
    [InlineData(nameof(ProfesionalDTO.Area))]
    [InlineData(nameof(ProfesionalDTO.Cuit))]
    [InlineData(nameof(ProfesionalDTO.MP))]
    [InlineData(nameof(ProfesionalDTO.RNP))]
    [InlineData(nameof(ProfesionalDTO.Telefono))]
    [InlineData(nameof(ProfesionalDTO.Email))]
    public void CampoObligatorioVacio_ProduceErrorEnEseCampo(string campo)
    {
        var dto = CrearValido();
        typeof(ProfesionalDTO).GetProperty(campo)!.SetValue(dto, "");
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Fact]
    public void DatosValidos_NoProducenErrores()
    {
        Assert.Empty(Validar(CrearValido()));
    }

    [Theory]
    [InlineData(nameof(ProfesionalDTO.Nombre), 101)]
    [InlineData(nameof(ProfesionalDTO.Area), 101)]
    [InlineData(nameof(ProfesionalDTO.MP), 31)]
    [InlineData(nameof(ProfesionalDTO.RNP), 31)]
    public void CampoExcedeLongitud_ProduceError(string campo, int longitud)
    {
        var dto = CrearValido();
        typeof(ProfesionalDTO).GetProperty(campo)!.SetValue(dto, new string('A', longitud));
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(campo));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("20987654323")]
    public void CuitInvalido_ProduceError(string cuit)
    {
        var dto = CrearValido();
        dto.Cuit = cuit;
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(nameof(dto.Cuit)));
    }

    [Fact]
    public void EmailSinFormatoValido_ProduceError()
    {
        var dto = CrearValido();
        dto.Email = "correo-sin-arroba";
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(nameof(dto.Email)));
    }

    [Fact]
    public void TelefonoConLetras_ProduceError()
    {
        var dto = CrearValido();
        dto.Telefono = "telefonoABC";
        Assert.Contains(Validar(dto), e => e.MemberNames.Contains(nameof(dto.Telefono)));
    }

    private static ProfesionalDTO CrearValido() => new()
    {
        Nombre = "Profesional Prueba", Area = "Psicología",
        Cuit = "20987654326", MP = "MP-123", RNP = "RNP-123",
        Telefono = "3511234567", Email = "prueba@example.com"
    };

    private static List<ValidationResult> Validar(object dto)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), errores, true);
        return errores;
    }
}

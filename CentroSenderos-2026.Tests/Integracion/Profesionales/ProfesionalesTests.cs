using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class ProfesionalesTests
{
    [Fact]
    public async Task InsertarProfesional_PermiteRecuperarSusDatos()
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion =
            await contexto.Database.BeginTransactionAsync();

        try
        {
            // Preparar un área exclusiva para esta prueba.
            var identificador = Guid.NewGuid().ToString("N")[..12];

            var area = new TipoArea
            {
                Tipo = $"Area Prueba {identificador}",
                Descripcion = "Área creada por una prueba automática",
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            contexto.TipoAreas.Add(area);
            await contexto.SaveChangesAsync();

            // CUIT ficticio con dígito verificador correcto.
            const string cuit = "20987654326";

            Assert.False(
                await contexto.Profesionales.AnyAsync(p =>
                    p.Cuit.Replace("-", "")
                        .Replace(".", "")
                        .Replace(" ", "") == cuit),
                "El CUIT de prueba ya existe. Hay que usar otro.");

            var datos = new ProfesionalDTO
            {
                Nombre = "Profesional Prueba",
                Area = area.Tipo,
                Cuit = cuit,
                MP = $"MP-{identificador}",
                RNP = $"RNP-{identificador}",
                Telefono = "3511234567",
                Email = $"prueba-{identificador}@example.com"
            };

            var repositorio = new ProfesionalRepositorio(contexto);

            // Ejecutar el alta usando el código real del sistema.
            var id = await repositorio.InsertarProfesional(datos);

            // Consultar nuevamente desde la base.
            contexto.ChangeTracker.Clear();

            var recuperado = await repositorio.SelectPorId(id);

            // Comprobar el resultado.
            Assert.True(id > 0);
            Assert.NotNull(recuperado);
            Assert.Equal(datos.Nombre, recuperado.Nombre);
            Assert.Equal(datos.Area, recuperado.Area);
            Assert.Equal(datos.Cuit, recuperado.Cuit);
            Assert.Equal(datos.MP.ToUpperInvariant(), recuperado.MP);
            Assert.Equal(datos.RNP.ToUpperInvariant(), recuperado.RNP);
            Assert.Equal(datos.Telefono, recuperado.Telefono);
            Assert.Equal(datos.Email, recuperado.Email);
            Assert.Equal(
                EnumEstadoRegistro.activo,
                recuperado.EstadoRegistro);
        }
        finally
        {
            // Deshacer los registros aunque falle una comprobación.
            await transaccion.RollbackAsync();
        }
    }
}
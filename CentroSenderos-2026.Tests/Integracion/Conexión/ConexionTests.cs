using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace CentroSenderos_2026.Tests.Integracion.Conexión;

public class ConexionTests
{
    [Fact]
    public async Task BaseDeTesting_PermiteConectarse()
    {
        // Leer la conexión desde los secretos de usuario.
        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<ConexionTests>()
            .Build();

        var cadena = configuracion.GetConnectionString("TestingConnection");

        Assert.False(
            string.IsNullOrWhiteSpace(cadena),
            "Falta configurar TestingConnection en los secretos de usuario.");

        // Evitar ejecutar esta prueba contra otro servidor.
        var datosConexion = new NpgsqlConnectionStringBuilder(cadena);

        Assert.Equal(
            "ep-divine-pine-atc5ncxu-pooler.c-9.us-east-1.aws.neon.tech",
            datosConexion.Host);

        // Abrir la conexión y ejecutar una consulta sin modificar datos.
        await using var conexion = new NpgsqlConnection(cadena);
        await conexion.OpenAsync();

        await using var comando = new NpgsqlCommand("SELECT 1", conexion);
        var resultado = await comando.ExecuteScalarAsync();

        Assert.Equal(1, Assert.IsType<int>(resultado));
    }
}
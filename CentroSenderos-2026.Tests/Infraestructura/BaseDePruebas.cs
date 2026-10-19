using CentroSenderos_2026.Tests.Integracion.Conexión;
using CentroSenderos_2026_BD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CentroSenderos_2026.Tests.Infraestructura;

public static class BaseDePruebas
{
    public static ApplicationDbContext CrearContexto()
    {
        // Leer únicamente la conexión destinada a pruebas.
        var configuracion = new ConfigurationBuilder()
            .AddUserSecrets<ConexionTests>()
            .Build();

        var cadena = configuracion.GetConnectionString(
            "TestingConnection");

        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new InvalidOperationException(
                "Falta configurar TestingConnection.");
        }

        // Impedir el uso accidental de otro servidor.
        var conexion = new NpgsqlConnectionStringBuilder(cadena);

        const string hostTesting =
            "ep-divine-pine-atc5ncxu-pooler.c-9.us-east-1.aws.neon.tech";

        if (!string.Equals(
            conexion.Host,
            hostTesting,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La conexión no corresponde al servidor de testing.");
        }

        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(cadena)
            .Options;

        return new ApplicationDbContext(opciones);
    }
}
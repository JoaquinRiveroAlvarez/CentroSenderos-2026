using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Validaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class OperacionesAnonimasHttpTests
{
    [Theory]
    [InlineData("paciente", "listar")]
    [InlineData("paciente", "consultar")]
    [InlineData("paciente", "crear")]
    [InlineData("paciente", "editar")]
    [InlineData("paciente", "eliminar")]
    [InlineData("profesional", "listar")]
    [InlineData("profesional", "consultar")]
    [InlineData("profesional", "crear")]
    [InlineData("profesional", "editar")]
    [InlineData("profesional", "eliminar")]
    [Trait("Categoria", "SeguridadHttp")]
    public async Task SinSesion_LaOperacionDebeExigirAutenticacion(string recurso, string operacion)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<OperacionesAnonimasHttpTests>().Build();
        var configServer = new ConfigurationBuilder()
            .AddUserSecrets("aspnet-CentroSenderos_2026_Server-ebda0453-8a02-44d5-a115-ed06d0d5bec7").Build();
        var testing = config.GetConnectionString("TestingConnection");
        var servidor = configServer.GetConnectionString("DefaultConnection");
        Assert.False(string.IsNullOrWhiteSpace(testing), "Falta TestingConnection.");
        Assert.False(string.IsNullOrWhiteSpace(servidor), "Falta DefaultConnection en secretos del Server.");
        var destinoTesting = new NpgsqlConnectionStringBuilder(testing);
        var destinoServer = new NpgsqlConnectionStringBuilder(servidor);
        Assert.True(string.Equals(destinoTesting.Host, destinoServer.Host, StringComparison.OrdinalIgnoreCase)
            && destinoTesting.Port == destinoServer.Port
            && destinoTesting.Database == destinoServer.Database
            && destinoTesting.Username == destinoServer.Username,
            "Los secretos del Server y Tests apuntan a destinos diferentes. Detener y revisar conexiones.");
        // CrearContexto también valida el host autorizado para testing.
        await using var db = BaseDePruebas.CrearContexto();


        var direccion = Environment.GetEnvironmentVariable("CENTROSENDEROS_TEST_URL") ?? "https://localhost:7069";
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var url) || !url.IsLoopback || url.Scheme != "https")
            throw new InvalidOperationException("Se requiere una URL local HTTPS.");
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false };
        using var cliente = new HttpClient(handler)
        {
            BaseAddress = new Uri(url.GetLeftPart(UriPartial.Authority)),
            Timeout = TimeSpan.FromSeconds(60)
        };
        // No se envían cookies, tokens ni credenciales de usuario.
        string dni;
        do { dni = Random.Shared.Next(80000000, 99000000).ToString(); }
        while (await db.Pacientes.AnyAsync(p => p.DNI == dni));
        string cuit;
        do { cuit = GenerarCuit(); }
        while (await db.Profesionales.AnyAsync(p => p.Cuit == cuit));
        var marca = Guid.NewGuid().ToString("N")[..12];
        var correo = "seguridad." + marca + "@example.invalid";
        var pacienteRepo = new PacienteRepositorio(db);
        var profesionalRepo = new ProfesionalRepositorio(db);
        var id = 0;
        object dto;

        try
        {
            if (recurso == "paciente")
            {
                var obra = await db.TipoObrasSociales.Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo)
                    .Select(x => x.Id).FirstAsync();
                var diagnostico = await db.TipoDiagnosticos.Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo)
                    .Select(x => x.Id).FirstAsync();
                var nuevo = new PacienteCrearDTO
                {
                    Nombre = "Paciente Seguridad " + dni, DNI = dni,
                    FechaNacimiento = new DateTime(2015, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    Domicilio = "Domicilio ficticio de testing", Telefono = "3510000000",
                    TipoObraSocialId = obra, TipoDiagnosticoId = diagnostico,
                    Telefonos = [new() { Etiqueta = "Paciente", Numero = "3510000000" }]
                };
                dto = nuevo;
                if (operacion != "crear")
                {
                    id = await pacienteRepo.InsertarPaciente(nuevo);
                    var editar = (await pacienteRepo.SelectPorId(id))!;
                    editar.Nombre = "Paciente Modificado " + dni;
                    dto = editar;
                }
            }
            else
            {
                var area = await db.TipoAreas.Where(x => x.EstadoRegistro == EnumEstadoRegistro.activo)
                    .Select(x => x.Tipo).FirstAsync();
                var nuevo = new ProfesionalDTO
                {
                    Nombre = "Profesional Seguridad " + cuit, Cuit = cuit, Area = area,
                    MP = "SEG-MP-" + marca.ToUpperInvariant(), RNP = "SEG-RNP-" + marca.ToUpperInvariant(),
                    Telefono = "3510000000", Email = correo, RolAsignado = ""
                };
                dto = nuevo;
                if (operacion != "crear")
                {
                    id = await profesionalRepo.InsertarProfesional(nuevo);
                    nuevo.Nombre = "Profesional Modificado " + cuit;
                }
            }

            var lista = recurso == "paciente" ? "ListaPaciente" : "ListaProfesional";
            var ruta = operacion == "listar" ? "/api/" + recurso + "/" + lista
                : operacion == "crear" ? "/api/" + recurso + (recurso == "paciente" ? "/insertar" : "")
                : "/api/" + recurso + "/" + id;
            using var respuesta = operacion switch
            {
                "crear" => await cliente.PostAsJsonAsync(ruta, dto),
                "editar" => await cliente.PutAsJsonAsync(ruta, dto),
                "eliminar" => await cliente.DeleteAsync(ruta),
                _ => await cliente.GetAsync(ruta)
            };
            var protegido = respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
            var destino = respuesta.Headers.Location;
            if ((int)respuesta.StatusCode is 301 or 302 or 303 or 307 or 308 && destino != null)
            {
                var absoluta = destino.IsAbsoluteUri ? destino : new Uri(cliente.BaseAddress!, destino);
                protegido = absoluta.AbsolutePath.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase);
            }
            var mensaje = recurso + " / " + operacion + " sin sesión recibió HTTP "
                + (int)respuesta.StatusCode + ". Se esperaba 401, 403 o redirección al login.";
            if (respuesta.IsSuccessStatusCode && operacion is "crear" or "editar" or "eliminar")
            {
                // Confirmar el efecto real sin registrar nombres ni datos de pacientes existentes.
                await using var verificacion = BaseDePruebas.CrearContexto();
                if (recurso == "paciente")
                {
                    var propio = await verificacion.Pacientes.AsNoTracking().SingleOrDefaultAsync(p => p.DNI == dni);
                    mensaje += " Registro ficticio: " + (propio == null ? "ausente" :
                        "presente; estado=" + propio.EstadoRegistro
                        + "; edición aplicada=" + (propio.Nombre == "Paciente Modificado " + dni));
                }
                else
                {
                    var propio = await verificacion.Profesionales.AsNoTracking()
                        .SingleOrDefaultAsync(p => p.Cuit == cuit && p.Email == correo);
                    mensaje += " Registro ficticio: " + (propio == null ? "ausente" :
                        "presente; estado=" + propio.EstadoRegistro
                        + "; edición aplicada=" + (propio.Nombre == "Profesional Modificado " + cuit));
                }
            }
            Assert.True(protegido, mensaje);
        }
        finally
        {
            // Baja lógica solo de los datos de esta ejecución, también si la prueba queda roja.
            await using var limpieza = BaseDePruebas.CrearContexto();
            if (recurso == "paciente")
            {
                var propio = await limpieza.Pacientes.SingleOrDefaultAsync(p => p.DNI == dni
                    && (p.Nombre == "Paciente Seguridad " + dni || p.Nombre == "Paciente Modificado " + dni));
                if (propio != null) propio.EstadoRegistro = EnumEstadoRegistro.borrado;
            }
            else
            {
                var propio = await limpieza.Profesionales.SingleOrDefaultAsync(p => p.Cuit == cuit && p.Email == correo);
                if (propio != null) propio.EstadoRegistro = EnumEstadoRegistro.borrado;
            }
            await limpieza.SaveChangesAsync();
        }
    }

    private static string GenerarCuit()
    {
        // Identificador sintético con dígito verificador; no implica identidad real.
        while (true)
        {
            var baseCuit = "20" + Random.Shared.Next(80000000, 99000000);
            int[] pesos = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
            var suma = baseCuit.Select((digito, i) => (digito - '0') * pesos[i]).Sum();
            var verificador = 11 - suma % 11;
            if (verificador == 11) verificador = 0;
            if (verificador == 10) continue;
            var candidato = baseCuit + verificador;
            if (CuitValidador.EsValido(candidato)) return candidato;
        }
    }


}

using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
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

            var cuit = await CrearCuitDisponible(contexto);

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

    [Theory]
    [InlineData("CUIT")]
    [InlineData("MP")]
    [InlineData("RNP")]
    public async Task InsertarProfesional_RechazaDuplicados(string campo)
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            await repositorio.InsertarProfesional(datos);
            var segundo = await CrearDatos(contexto, datos.Area);
            if (campo == "CUIT") segundo.Cuit = datos.Cuit;
            if (campo == "MP") segundo.MP = datos.MP;
            if (campo == "RNP") segundo.RNP = datos.RNP;

            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repositorio.InsertarProfesional(segundo); });
            Assert.Contains("Ya existe un profesional", error.Message);
            Assert.Contains(campo == "MP" ? "Matrícula Profesional" : campo,
                error.Message);
            Assert.False(await contexto.Profesionales.AnyAsync(p => p.Email == segundo.Email));
        });
    }

    [Fact]
    public async Task InsertarProfesional_RechazaCuitInvalido()
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            datos.Cuit = "123";
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repositorio.InsertarProfesional(datos); });
            Assert.Equal("El CUIT ingresado no es válido.", error.Message);
            Assert.False(await contexto.Profesionales.AnyAsync(p => p.Email == datos.Email));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InsertarProfesional_RechazaAreaVaciaOInexistente(bool vacia)
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            datos.Area = vacia ? "" : $"NoExiste-{Guid.NewGuid():N}";
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repositorio.InsertarProfesional(datos); });
            Assert.Equal(vacia ? "Seleccioná un área profesional."
                : "El área seleccionada no existe o ya no está disponible.", error.Message);
            Assert.False(await contexto.Profesionales.AnyAsync(p => p.Email == datos.Email));
        });
    }

    [Fact]
    public async Task ActualizarProfesional_PersisteLosCambios()
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            var id = await repositorio.InsertarProfesional(datos);
            datos.Nombre = "Profesional Actualizado";
            datos.Telefono = "3517654321";
            datos.Email = $"actualizado-{Guid.NewGuid():N}@example.com";
            Assert.True(await repositorio.ActualizarProfesional(id, datos));
            contexto.ChangeTracker.Clear();
            var guardado = await repositorio.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(datos.Nombre, guardado.Nombre);
            Assert.Equal(datos.Telefono, guardado.Telefono);
            Assert.Equal(datos.Email, guardado.Email);
            Assert.Equal(datos.Cuit, guardado.Cuit);
        });
    }

    [Fact]
    public async Task DeleteProfesional_RealizaBajaLogicaYExcluyeDelListado()
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            var id = await repositorio.InsertarProfesional(datos);
            Assert.True(await repositorio.DeleteProfesional(id));
            contexto.ChangeTracker.Clear();
            var guardado = await contexto.Profesionales.AsNoTracking()
                .SingleAsync(p => p.Id == id);
            Assert.Equal(EnumEstadoRegistro.borrado, guardado.EstadoRegistro);
            Assert.DoesNotContain(await repositorio.SelectListaProfesional(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task SelectPorId_DevuelveNullSiNoExiste()
    {
        await EnTransaccion(async (contexto, repositorio, datos) =>
        {
            Assert.False(await contexto.Profesionales.AnyAsync(p => p.Id == -1));
            Assert.Null(await repositorio.SelectPorId(-1));
        });
    }

    // Preparar datos exclusivos y revertirlos incluso si una aserción falla.
    private static async Task EnTransaccion(
        Func<ApplicationDbContext, ProfesionalRepositorio, ProfesionalDTO, Task> prueba)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        try
        {
            var area = new TipoArea
            {
                Tipo = $"Area Prueba {Guid.NewGuid():N}",
                Descripcion = "Datos de prueba automática",
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            contexto.TipoAreas.Add(area);
            await contexto.SaveChangesAsync();
            var datos = await CrearDatos(contexto, area.Tipo);
            await prueba(contexto, new ProfesionalRepositorio(contexto), datos);
        }
        finally
        {
            await transaccion.RollbackAsync();
        }
    }

    private static async Task<ProfesionalDTO> CrearDatos(ApplicationDbContext contexto, string area)
    {
        var sufijo = Guid.NewGuid().ToString("N")[..12];
        return new ProfesionalDTO
        {
            Nombre = "Profesional Prueba",
            Area = area,
            Cuit = await CrearCuitDisponible(contexto),
            MP = $"MP-{sufijo}",
            RNP = $"RNP-{sufijo}",
            Telefono = "3511234567",
            Email = $"prueba-{sufijo}@example.com"
        };
    }

    private static async Task<string> CrearCuitDisponible(ApplicationDbContext contexto)
    {
        // Datos sintéticos con dígito verificador válido, sin depender de un CUIT fijo.
        int[] pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        for (var intento = 0; intento < 100; intento++)
        {
            var baseCuit = "20" + Random.Shared.Next(10000000, 100000000);
            var suma = 0;
            for (var i = 0; i < pesos.Length; i++)
                suma += (baseCuit[i] - '0') * pesos[i];
            var digito = 11 - suma % 11;
            if (digito == 10) continue;
            if (digito == 11) digito = 0;
            var cuit = baseCuit + digito;
            if (!await contexto.Profesionales.AnyAsync(p =>
                p.Cuit.Replace("-", "").Replace(".", "").Replace(" ", "") == cuit))
                return cuit;
        }
        throw new InvalidOperationException("No se pudo generar un CUIT de prueba disponible.");
    }

}

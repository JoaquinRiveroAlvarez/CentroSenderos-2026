using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace CentroSenderos_2026.Tests;

public class SociosTests
{
    [Fact]
    public async Task Alta_VinculaProfesionalYRecuperaObservacion()
    {
        await ConProfesional(async (db, repo, dto) =>
        {
            var id = await repo.InsertarSocio(dto);
            db.ChangeTracker.Clear();
            var socio = await repo.SelectPorId(id);
            Assert.NotNull(socio);
            Assert.Equal(dto.ProfesionalId, socio.ProfesionalId);
            Assert.Equal(dto.Observacion, socio.Observacion);
            Assert.False(socio.EsCaja);
            Assert.Contains(await repo.SelectListaSocios(), s => s.Id == id);
        });
    }
    [Fact]
    public async Task Edicion_PersisteObservacion()
    {
        await ConProfesional(async (db, repo, dto) =>
        {
            var id = await repo.InsertarSocio(dto);
            dto.Observacion = "Observación actualizada";
            Assert.True(await repo.ActualizarSocio(id, dto));
            db.ChangeTracker.Clear();
            var socio = await repo.SelectPorId(id);
            Assert.NotNull(socio);
            Assert.Equal(dto.Observacion, socio.Observacion);
            Assert.Equal(dto.ProfesionalId, socio.ProfesionalId);
        });
    }
    [Fact]
    public async Task Baja_EsLogicaYConservaProfesional()
    {
        await ConProfesional(async (db, repo, dto) =>
        {
            var id = await repo.InsertarSocio(dto);
            Assert.True(await repo.DeleteSocio(id));
            db.ChangeTracker.Clear();
            Assert.Equal(EnumEstadoRegistro.borrado,
                (await db.Socios.SingleAsync(s => s.Id == id)).EstadoRegistro);
            Assert.DoesNotContain(await repo.SelectListaSocios(), s => s.Id == id);
            Assert.True(await db.Profesionales.AnyAsync(p => p.Id == dto.ProfesionalId));
        });
    }
    [Fact]
    public async Task Alta_RechazaProfesionalYaAsociado()
    {
        await ConProfesional(async (db, repo, dto) =>
        {
            await repo.InsertarSocio(dto);
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarSocio(dto); });
            Assert.Equal("El profesional seleccionado ya tiene un registro de socio.", error.Message);
            Assert.Equal(1, await db.Socios.CountAsync(s => s.ProfesionalId == dto.ProfesionalId));
        });
    }
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Alta_RechazaProfesionalNoSeleccionado(int? profesionalId)
    {
        await using var db = BaseDePruebas.CrearContexto();
        var repo = new SocioRepositorio(db);
        var error = await Assert.ThrowsAsync<ApplicationException>(async () =>
        {
            await repo.InsertarSocio(new SocioDTO { ProfesionalId = profesionalId });
        });
        Assert.Equal("Seleccioná un profesional para el socio.", error.Message);
    }

    // El repositorio administra sus propias transacciones.
    // Se limpian exclusivamente los IDs creados por este caso, en orden de relación.
    private static async Task ConProfesional(
        Func<ApplicationDbContext, SocioRepositorio, SocioDTO, Task> prueba)
    {
        await using var db = BaseDePruebas.CrearContexto();
        var sufijo = Guid.NewGuid().ToString("N")[..12];
        string cuit;
        int[] pesos = {5,4,3,2,7,6,5,4,3,2};
        do
        {
            var numero = "20" + Random.Shared.Next(10000000, 100000000);
            var suma = 0;
            for (int i = 0; i < 10; i++) suma += (numero[i] - '0') * pesos[i];
            var digito = 11 - suma % 11;
            if (digito == 10) { cuit = ""; continue; }
            cuit = numero + (digito == 11 ? 0 : digito);
        } while (cuit == "" || await db.Profesionales.AnyAsync(p =>
            p.Cuit.Replace("-", "").Replace(".", "").Replace(" ", "") == cuit));

        var profesional = new Profesional
        {
            Nombre = "Profesional Prueba Socios", Area = "Área de prueba",
            Cuit = cuit, MP = $"MP-{sufijo}", RNP = $"RNP-{sufijo}",
            Telefono = "3511234567", Email = $"socio-{sufijo}@example.com",
            EstadoRegistro = EnumEstadoRegistro.activo
        };
        try
        {
            db.Profesionales.Add(profesional);
            await db.SaveChangesAsync();
            await prueba(db, new SocioRepositorio(db), new SocioDTO
            {
                ProfesionalId = profesional.Id,
                Observacion = "Socio de prueba",
                EsCaja = false, EstadoRegistro = EnumEstadoRegistro.activo
            });
        }
        finally
        {
            if (profesional.Id > 0)
            {
                db.ChangeTracker.Clear();
                await db.Socios.Where(s => s.ProfesionalId == profesional.Id)
                    .ExecuteDeleteAsync();
                await db.Profesionales.Where(p => p.Id == profesional.Id)
                    .ExecuteDeleteAsync();
            }
        }
    }
}

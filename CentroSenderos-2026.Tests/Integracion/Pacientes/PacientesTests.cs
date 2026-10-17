using CentroSenderos_2026.Tests.Infraestructura;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CentroSenderos_2026.Tests;

public class PacientesTests
{
    [Fact]
    public async Task InsertarPaciente_RecuperaDatosTelefonosYRelaciones()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarPaciente(datos);
            contexto.ChangeTracker.Clear();
            var paciente = await repo.SelectPorId(id);
            Assert.True(id > 0);
            Assert.NotNull(paciente);
            Assert.Equal(datos.Nombre, paciente.Nombre);
            Assert.Equal(datos.DNI, paciente.DNI);
            Assert.Equal(datos.FechaNacimiento, paciente.FechaNacimiento);
            Assert.Equal(datos.TieneCud, paciente.TieneCud);
            Assert.Equal(datos.Domicilio, paciente.Domicilio);
            Assert.Equal(datos.TipoObraSocialId, paciente.TipoObraSocialId);
            Assert.Equal(datos.TipoDiagnosticoId, paciente.TipoDiagnosticoId);
            Assert.True(paciente.NumeroAfiliado > 0);
            Assert.Equal(2, paciente.Telefonos.Count);
            foreach (var telefono in datos.Telefonos)
                Assert.Contains(paciente.Telefonos, t =>
                    t.Numero == telefono.Numero && t.Etiqueta == telefono.Etiqueta);
            Assert.Equal(datos.Telefonos[0].Numero, paciente.Telefono);
        });
    }

    [Fact]
    public async Task InsertarPaciente_RechazaDniDuplicadoInclusoConPuntos()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            await repo.InsertarPaciente(datos);
            datos.DNI = datos.DNI.Insert(5, ".").Insert(2, ".");
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarPaciente(datos); });
            Assert.Contains("Ya existe un paciente con el DNI", error.Message);
        });
    }

    [Fact]
    public async Task InsertarPaciente_RechazaAusenciaDeTelefonos()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            datos.Telefono = "";
            datos.Telefonos.Clear();
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarPaciente(datos); });
            Assert.Equal("Debe registrar al menos un teléfono.", error.Message);
            Assert.False(await contexto.Pacientes.AnyAsync(p => p.DNI == datos.DNI));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InsertarPaciente_RechazaRelacionInexistente(bool obraSocial)
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            if (obraSocial)
            {
                Assert.False(await contexto.TipoObrasSociales.AnyAsync(o => o.Id == -1));
                datos.TipoObraSocialId = -1;
            }
            else
            {
                Assert.False(await contexto.TipoDiagnosticos.AnyAsync(d => d.Id == -1));
                datos.TipoDiagnosticoId = -1;
            }
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.InsertarPaciente(datos); });
            Assert.Equal(obraSocial ? "La obra social seleccionada no existe."
                : "El diagnóstico seleccionado no existe.", error.Message);
            Assert.False(await contexto.Pacientes.AnyAsync(p => p.DNI == datos.DNI));
        });
    }

    [Fact]
    public async Task InsertarPaciente_AdmiteTelefonoDelFormularioAnterior()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            datos.Telefonos.Clear();
            datos.Telefono = "3515551234";
            var id = await repo.InsertarPaciente(datos);
            contexto.ChangeTracker.Clear();
            var paciente = await repo.SelectPorId(id);
            Assert.NotNull(paciente);
            var telefono = Assert.Single(paciente.Telefonos);
            Assert.Equal(datos.Telefono, telefono.Numero);
            Assert.Equal("Principal", telefono.Etiqueta);
        });
    }

    [Fact]
    public async Task ActualizarPaciente_PersisteDatosYReemplazaTelefonos()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarPaciente(datos);
            var editar = await repo.SelectPorId(id);
            Assert.NotNull(editar);
            var afiliado = editar.NumeroAfiliado;
            editar.Nombre = "Paciente Actualizado";
            editar.Domicilio = "Calle Prueba 456";
            editar.TieneCud = false;
            editar.Telefonos = new()
            {
                new PacienteTelefonoDTO { Numero = "3515559999", Etiqueta = "Personal" }
            };
            Assert.True(await repo.ActualizarPaciente(id, editar));
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(editar.Nombre, guardado.Nombre);
            Assert.Equal(editar.Domicilio, guardado.Domicilio);
            Assert.False(guardado.TieneCud);
            Assert.Equal(afiliado, guardado.NumeroAfiliado);
            Assert.Equal("3515559999", Assert.Single(guardado.Telefonos).Numero);
            Assert.Single(await contexto.PacienteTelefonos
                .Where(t => t.PacienteId == id).ToListAsync());
        });
    }

    [Fact]
    public async Task DeletePaciente_RealizaBajaLogicaYOcultaElRegistro()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarPaciente(datos);
            Assert.True(await repo.DeletePaciente(id));
            contexto.ChangeTracker.Clear();
            var entidad = await contexto.Pacientes.AsNoTracking().SingleAsync(p => p.Id == id);
            Assert.Equal(EnumEstadoRegistro.borrado, entidad.EstadoRegistro);
            Assert.Null(await repo.SelectPorId(id));
            Assert.DoesNotContain(await repo.SelectListaPaciente(), p => p.Id == id);
        });
    }

    [Fact]
    public async Task SelectPorId_DevuelveNullSiNoExiste()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            Assert.False(await contexto.Pacientes.AnyAsync(p => p.Id == -1));
            Assert.Null(await repo.SelectPorId(-1));
        });
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Edicion_RechazaDniONumeroAfiliadoDeOtroPaciente(bool duplicarDni)
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var primeroId = await repo.InsertarPaciente(datos);
            var primero = await repo.SelectPorId(primeroId);
            Assert.NotNull(primero);
            string otroDni;
            do { otroDni = Random.Shared.Next(10000000, 100000000).ToString(); }
            while (await contexto.Pacientes.AnyAsync(p => p.DNI == otroDni));
            datos.DNI = otroDni;
            var segundoId = await repo.InsertarPaciente(datos);
            var segundo = await repo.SelectPorId(segundoId);
            Assert.NotNull(segundo);
            var afiliadoOriginal = segundo.NumeroAfiliado;
            if (duplicarDni) segundo.DNI = primero.DNI;
            else segundo.NumeroAfiliado = primero.NumeroAfiliado;

            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.ActualizarPaciente(segundoId, segundo); });
            Assert.Contains(duplicarDni ? "DNI" : "número de afiliado", error.Message);
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(segundoId);
            Assert.NotNull(guardado);
            Assert.Equal(otroDni, guardado.DNI);
            Assert.Equal(afiliadoOriginal, guardado.NumeroAfiliado);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Edicion_RechazaObraSocialODiagnosticoInexistente(bool obraSocial)
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarPaciente(datos);
            var editar = await repo.SelectPorId(id);
            Assert.NotNull(editar);
            if (obraSocial)
            {
                Assert.False(await contexto.TipoObrasSociales.AnyAsync(o => o.Id == -1));
                editar.TipoObraSocialId = -1;
            }
            else
            {
                Assert.False(await contexto.TipoDiagnosticos.AnyAsync(d => d.Id == -1));
                editar.TipoDiagnosticoId = -1;
            }
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.ActualizarPaciente(id, editar); });
            Assert.Equal(obraSocial ? "La obra social seleccionada no existe."
                : "El diagnóstico seleccionado no existe.", error.Message);
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(datos.TipoObraSocialId, guardado.TipoObraSocialId);
            Assert.Equal(datos.TipoDiagnosticoId, guardado.TipoDiagnosticoId);
            Assert.Equal(2, guardado.Telefonos.Count);
        });
    }

    [Fact]
    public async Task Edicion_SinTelefonosRechazaYConservaLosAnteriores()
    {
        await EnTransaccion(async (contexto, repo, datos) =>
        {
            var id = await repo.InsertarPaciente(datos);
            var editar = await repo.SelectPorId(id);
            Assert.NotNull(editar);
            editar.Telefono = "";
            editar.Telefonos.Clear();
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => { await repo.ActualizarPaciente(id, editar); });
            Assert.Equal("Debe registrar al menos un teléfono.", error.Message);
            contexto.ChangeTracker.Clear();
            var guardado = await repo.SelectPorId(id);
            Assert.NotNull(guardado);
            Assert.Equal(2, guardado.Telefonos.Count);
            foreach (var telefono in datos.Telefonos)
                Assert.Contains(guardado.Telefonos, t =>
                    t.Numero == telefono.Numero && t.Etiqueta == telefono.Etiqueta);
        });
    }

    // Las escrituras se limitan a la conexión validada por BaseDePruebas.
    // No se aplican migraciones ni se limpian tablas existentes.
    private static async Task EnTransaccion(
        Func<ApplicationDbContext, PacienteRepositorio, PacienteCrearDTO, Task> prueba)
    {
        await using var contexto = BaseDePruebas.CrearContexto();
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        try
        {
            var sufijo = Guid.NewGuid().ToString("N");
            var obra = new TipoObraSocial
            {
                Tipo = $"Obra Prueba {sufijo}",
                Descripcion = "Datos de testing",
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            var diagnostico = new TipoDiagnostico
            {
                Tipo = $"Diagnostico Prueba {sufijo}",
                Descripcion = "Datos de testing",
                EstadoRegistro = EnumEstadoRegistro.activo
            };
            contexto.TipoObrasSociales.Add(obra);
            contexto.TipoDiagnosticos.Add(diagnostico);
            await contexto.SaveChangesAsync();

            string dni;
            do
            {
                dni = Random.Shared.Next(10000000, 100000000).ToString();
            }
            while (await contexto.Pacientes.AnyAsync(p => p.DNI == dni));

            var datos = new PacienteCrearDTO
            {
                Nombre = "Paciente Prueba",
                DNI = dni,
                FechaNacimiento = new DateTime(2000, 1, 15),
                TieneCud = true,
                Domicilio = "Calle Prueba 123",
                TipoObraSocialId = obra.Id,
                TipoDiagnosticoId = diagnostico.Id,
                Telefonos = new()
                {
                    new PacienteTelefonoDTO { Numero = "3511234567", Etiqueta = "Personal" },
                    new PacienteTelefonoDTO { Numero = "3517654321", Etiqueta = "Familiar" }
                }
            };
            await prueba(contexto, new PacienteRepositorio(contexto), datos);
        }
        finally
        {
            await transaccion.RollbackAsync();
        }
    }
}

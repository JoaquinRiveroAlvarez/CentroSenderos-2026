using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using CentroSenderos_2026_Shared.Presentacion;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026.Tests.Unitarias;

public class FiltrosEntidadesTests
{
    #region Edad y documentos

    [Theory]
    [InlineData(2021, 11, 18, "0-5")]
    [InlineData(2020, 11, 17, "6-12")]
    [InlineData(2013, 11, 18, "6-12")]
    [InlineData(2013, 11, 17, "13-17")]
    [InlineData(2008, 11, 18, "13-17")]
    [InlineData(2008, 11, 17, "18-59")]
    [InlineData(1966, 11, 18, "18-59")]
    [InlineData(1966, 11, 17, "60+")]
    [InlineData(2027, 1, 1, "sin-fecha")]
    public void EdadRespetaElCumpleanios(int anio, int mes, int dia, string esperado)
    {
        Assert.Equal(esperado, FiltrosEntidades.ObtenerRangoEdad(
            new DateTime(anio, mes, dia), new DateTime(2026, 11, 17)));
    }

    [Fact]
    public void SinNacimientoTieneSuPropiaOpcion()
    {
        Assert.Equal("sin-fecha", FiltrosEntidades.ObtenerRangoEdad(null, DateTime.Today));
    }

    [Fact]
    public void PacientesCombinanGruposYPermitenVariasOpcionesDentroDeCadaGrupo()
    {
        var paciente = new PacienteResumenDTO
        {
            FechaNacimiento = new DateTime(2010, 1, 1),
            TiposDocumentos = new() { new() { Id = 2, Tipo = "Estudio" } }
        };
        var hoy = new DateTime(2026, 11, 17);
        Assert.True(FiltrosEntidades.CoincidePaciente(paciente, new HashSet<string> { "6-12", "13-17" }, new HashSet<string> { "si" }, new HashSet<string> { "1", "2" }, hoy));
        Assert.False(FiltrosEntidades.CoincidePaciente(paciente, new HashSet<string>(), new HashSet<string> { "no" }, new HashSet<string>(), hoy));
        Assert.False(FiltrosEntidades.CoincidePaciente(paciente, new HashSet<string>(), new HashSet<string>(), new HashSet<string> { "1" }, hoy));
        Assert.True(FiltrosEntidades.CoincidePaciente(paciente, new HashSet<string>(), new HashSet<string>(), new HashSet<string>(), hoy));
    }

    [Fact]
    public async Task ListadoIncluyeSoloTiposDeDocumentosActivosSinContenido()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.TipoObrasSociales.Add(new TipoObraSocial { Id = 1, Tipo = "Particular", Cuit = "20123456786" });
        db.TipoDiagnosticos.Add(new TipoDiagnostico { Id = 1, Tipo = "Consulta" });
        db.TipoDocumentos.AddRange(new TipoDocumento { Id = 1, Tipo = "CUD" }, new TipoDocumento { Id = 2, Tipo = "Estudio" });
        db.Pacientes.Add(new Paciente
        {
            EstadoRegistro = EnumEstadoRegistro.activo, Id = 1, Nombre = "Paciente de prueba", DNI = "12345678", NumeroAfiliado = 1,
            Telefono = "12345678", Domicilio = "Calle 123", TipoObraSocialId = 1, TipoDiagnosticoId = 1
        });
        db.Documentos.AddRange(
            new Documento { EstadoRegistro = EnumEstadoRegistro.activo, PacienteId = 1, TipoDocumentoId = 1, Contenido = new byte[] { 1, 2, 3 } },
            new Documento { PacienteId = 1, TipoDocumentoId = 2, EstadoRegistro = EnumEstadoRegistro.inactivo });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var paciente = Assert.Single(await new PacienteRepositorio(db).SelectListaPaciente());
        Assert.Equal(1, Assert.Single(paciente.TiposDocumentos).Id);
        Assert.Empty(db.ChangeTracker.Entries<Documento>());
    }

    #endregion

    #region Turnos y búsqueda

    private static TurnoListadoDTO Turno() => new()
    {
        EstadoTurno = EnumEstadoTurno.reservado,
        ProfesionalPrestaciones = new()
        {
            new() { ProfesionalId = 1, TipoPrestacionId = 1, TipoAreaId = 1 },
            new() { ProfesionalId = 2, TipoPrestacionId = 2, TipoAreaId = 2 }
        }
    };

    [Fact]
    public void AreaYPrestacionCorrespondenALaMismaAtencion()
    {
        Assert.False(FiltrosEntidades.CoincideTurno(Turno(), new HashSet<string> { "1" }, new HashSet<string> { "2" }, new HashSet<string>()));
        Assert.True(FiltrosEntidades.CoincideTurno(Turno(), new HashSet<string> { "1", "2" }, new HashSet<string> { "2" }, new HashSet<string> { "reservado" }));
        Assert.False(FiltrosEntidades.CoincideTurno(Turno(), new HashSet<string>(), new HashSet<string>(), new HashSet<string> { "cancelado" }));
        Assert.True(FiltrosEntidades.CoincideTurno(Turno(), new HashSet<string>(), new HashSet<string>(), new HashSet<string>()));
    }

    [Fact]
    public void TurnoConPrestacionSinAreaSePuedeEncontrar()
    {
        var turno = Turno();
        turno.ProfesionalPrestaciones[0].TipoAreaId = null;
        Assert.True(FiltrosEntidades.CoincideTurno(turno, new HashSet<string> { "sin-area" }, new HashSet<string> { "1" }, new HashSet<string>()));
    }

    [Theory]
    [InlineData("Evaluación", " EVALUACION ", true)]
    [InlineData("Psicología — Infantil", "psicologia", true)]
    [InlineData(null, "consulta", false)]
    [InlineData(null, "", true)]
    public void BusquedaIgnoraTildesYEspacios(string? texto, string busqueda, bool esperado)
    {
        Assert.Equal(esperado, FiltrosEntidades.ContieneTexto(texto, busqueda));
    }

    #endregion
}

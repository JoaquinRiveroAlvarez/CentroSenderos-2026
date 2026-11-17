using System.Text.Json;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Server.Controllers;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Reglas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentroSenderos_2026.Tests.Unitarias;

public class AltaPacienteDocumentosTests
{
    private static async Task<ApplicationDbContext> CrearBase(SqliteConnection conexion)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(conexion).Options);
        await db.Database.EnsureCreatedAsync();
        db.TipoObrasSociales.Add(new TipoObraSocial { Id = 1, Tipo = "Particular", Cuit = "20123456786" });
        db.TipoDiagnosticos.Add(new TipoDiagnostico { Id = 1, Tipo = "Consulta" });
        db.TipoDocumentos.AddRange(new TipoDocumento { Id = 1, Tipo = "CUD" }, new TipoDocumento { Id = 2, Tipo = "Estudio" });
        await db.SaveChangesAsync();
        return db;
    }

    private static PacienteDocumentosController Controlador(ApplicationDbContext db) => new(
        new PacienteRepositorio(db), new DocumentoRepositorio(db), db, NullLogger<PacienteDocumentosController>.Instance);

    private static string Paciente() => JsonSerializer.Serialize(new PacienteCrearDTO
    {
        Nombre = "Ana Prueba", DNI = "12345678", Domicilio = "Calle 123", FechaNacimiento = new DateTime(2000, 1, 1),
        TipoObraSocialId = 1, TipoDiagnosticoId = 1, Telefono = "12345678",
        Telefonos = new() { new() { Etiqueta = "Principal", Numero = "12345678" } }
    });

    private static IFormFile Archivo(string nombre, byte[] contenido) => new FormFile(new MemoryStream(contenido), 0, contenido.Length, "archivos", nombre)
    { Headers = new HeaderDictionary(), ContentType = "application/pdf" };

    [Fact]
    public async Task Alta_GuardaPacienteYDocumentosConSuTipoYContenido()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        var resultado = await Controlador(db).Insertar(Paciente(), new() { 1, 2 },
            new() { Archivo("certificado.pdf", new byte[] { 1, 2, 3 }), Archivo("estudio.pdf", new byte[] { 4, 5 }) });
        var id = Assert.IsType<int>(Assert.IsType<OkObjectResult>(resultado.Result).Value);
        db.ChangeTracker.Clear();
        var paciente = await new PacienteRepositorio(db).SelectPorId(id);
        Assert.NotNull(paciente);
        Assert.Equal(2, paciente.Documentos.Count);
        var documentos = await db.Documentos.OrderBy(d => d.TipoDocumentoId).ToListAsync();
        Assert.All(documentos, d => Assert.Equal(id, d.PacienteId));
        Assert.Equal("certificado.pdf", documentos[0].NombreOriginal);
        Assert.Equal(new byte[] { 1, 2, 3 }, documentos[0].Contenido);
        Assert.Equal(new byte[] { 4, 5 }, documentos[1].Contenido);
        Assert.Equal("application/pdf", documentos[0].ContentType);
    }

    [Fact]
    public async Task Alta_SinAdjuntosEsValida()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        Assert.IsType<OkObjectResult>((await Controlador(db).Insertar(Paciente(), new(), new())).Result);
        Assert.Equal(1, await db.Pacientes.CountAsync());
        Assert.Empty(await db.Documentos.ToListAsync());
    }

    [Theory]
    [InlineData("tipo-inexistente")]
    [InlineData("sin-tipo")]
    [InlineData("archivo-vacio")]
    [InlineData("archivo-grande")]
    [InlineData("datos-invalidos")]
    [InlineData("muchos-archivos")]
    [InlineData("total-grande")]
    [InlineData("json-malformado")]
    public async Task AltaInvalida_NoGuardaDatos(string caso)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        var tipos = caso == "sin-tipo" ? new List<int>() : new() { caso == "tipo-inexistente" ? 999 : 1 };
        IFormFile archivo = caso switch
        {
            "archivo-vacio" => Archivo("vacio.pdf", Array.Empty<byte>()),
            "archivo-grande" => new FormFile(new MemoryStream(new byte[] { 1 }), 0, ReglasDocumentosPaciente.MaximoBytesPorArchivo + 1, "archivos", "grande.pdf"),
            _ => Archivo("estudio.pdf", new byte[] { 1 })
        };
        var archivos = new List<IFormFile> { archivo };
        if (caso == "muchos-archivos")
        {
            archivos = Enumerable.Range(0, 11).Select(i => Archivo($"archivo-{i}.pdf", new byte[] { 1 })).ToList();
            tipos = Enumerable.Repeat(1, 11).ToList();
        }
        if (caso == "total-grande")
        {
            archivos = Enumerable.Range(0, 2).Select(i => (IFormFile)new FormFile(new MemoryStream(new byte[] { 1 }), 0, 16 * 1024 * 1024, "archivos", $"archivo-{i}.pdf")).ToList();
            tipos = new() { 1, 1 };
        }
        var json = caso == "datos-invalidos" ? "{}" : caso == "json-malformado" ? "{texto" : Paciente();
        var resultado = await Controlador(db).Insertar(json, tipos, archivos);
        Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Empty(await db.Pacientes.ToListAsync());
        Assert.Empty(await db.Documentos.ToListAsync());
    }

    [Fact]
    public async Task FallaSegundoArchivo_ReviertePacientePrimerDocumentoYTelefonos()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        var resultado = await Controlador(db).Insertar(Paciente(), new() { 1, 2 },
            new() { Archivo("primero.pdf", new byte[] { 1, 2 }), new ArchivoRoto() });
        Assert.Equal(500, Assert.IsType<ObjectResult>(resultado.Result).StatusCode);
        db.ChangeTracker.Clear();
        Assert.Empty(await db.Pacientes.ToListAsync());
        Assert.Empty(await db.Documentos.ToListAsync());
        Assert.Empty(await db.PacienteTelefonos.ToListAsync());
    }

    [Fact]
    public async Task TipoCreadoDesdeFormulario_SePuedeUsarInmediatamenteEnElAlta()
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        var tipos = new TipoDocumentoRepositorio(db);
        var tipoId = await tipos.InsertarTipoDocumento(new TipoDocumentoDTO { Tipo = "  Informe médico  " });
        Assert.Equal("Informe médico", (await tipos.SelectPorId(tipoId))!.Tipo);
        var resultado = await Controlador(db).Insertar(Paciente(), new() { tipoId }, new() { Archivo("informe.pdf", new byte[] { 1, 2 }) });
        Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(tipoId, (await db.Documentos.SingleAsync()).TipoDocumentoId);
    }

    [Theory]
    [InlineData(" cud ")]
    [InlineData("  ")]
    public async Task TipoDuplicadoOVacio_NoSeCrea(string nombre)
    {
        await using var conexion = new SqliteConnection("Data Source=:memory:");
        await conexion.OpenAsync();
        await using var db = await CrearBase(conexion);
        var tipos = new TipoDocumentoRepositorio(db);
        await Assert.ThrowsAsync<ApplicationException>(() => tipos.InsertarTipoDocumento(new TipoDocumentoDTO { Tipo = nombre }));
        Assert.Equal(2, await db.TipoDocumentos.CountAsync());
    }

    private sealed class ArchivoRoto : IFormFile
    {
        public string ContentType => "application/pdf";
        public string ContentDisposition => "form-data";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length => 10;
        public string Name => "archivos";
        public string FileName => "segundo.pdf";
        public Stream OpenReadStream() => throw new IOException("Fallo simulado al leer el segundo archivo.");
        public void CopyTo(Stream target) => throw new IOException();
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => throw new IOException();
    }
}

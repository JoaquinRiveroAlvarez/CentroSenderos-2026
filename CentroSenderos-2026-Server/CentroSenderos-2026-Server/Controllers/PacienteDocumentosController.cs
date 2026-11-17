using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CentroSenderos_2026_BD;
using CentroSenderos_2026_Repositorio.Repositorios;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Reglas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Server.Controllers;

[Authorize]
[ApiController]
[Route("api/paciente")]
public class PacienteDocumentosController(
    IPacienteRepositorio pacientes,
    IDocumentoRepositorio documentos,
    ApplicationDbContext context,
    ILogger<PacienteDocumentosController> logger) : ControllerBase
{
    [HttpPost("insertar-con-documentos")]
    [RequestSizeLimit(ReglasDocumentosPaciente.MaximoBytesSolicitud)]
    [RequestFormLimits(MultipartBodyLengthLimit = ReglasDocumentosPaciente.MaximoBytesSolicitud)]
    public async Task<ActionResult<int>> Insertar(
        [FromForm] string pacienteJson,
        [FromForm] List<int> tipoDocumentoIds,
        [FromForm] List<IFormFile> archivos,
        CancellationToken cancellationToken = default)
    {
        PacienteCrearDTO? paciente;
        try
        {
            paciente = JsonSerializer.Deserialize<PacienteCrearDTO>(pacienteJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return BadRequest(new { mensaje = "Los datos del paciente no tienen un formato válido." });
        }
        if (paciente is null)
            return BadRequest(new { mensaje = "Completá los datos del paciente." });

        var errores = new List<ValidationResult>();
        var valido = Validator.TryValidateObject(paciente, new ValidationContext(paciente), errores, true);
        foreach (var telefono in paciente.Telefonos ?? new())
            valido = Validator.TryValidateObject(telefono, new ValidationContext(telefono), errores, true) && valido;
        if (!valido)
            return BadRequest(new { mensaje = string.Join(" ", errores.Select(e => e.ErrorMessage)) });
        if (paciente.FechaNacimiento > DateTime.Today)
            return BadRequest(new { mensaje = "La fecha de nacimiento no puede ser posterior a hoy." });
        if (archivos.Count != tipoDocumentoIds.Count || archivos.Count > ReglasDocumentosPaciente.MaximoArchivos)
            return BadRequest(new { mensaje = "Adjuntá hasta 10 archivos, cada uno con su tipo de documento." });
        if (archivos.Any(a => a.Length <= 0 || a.Length > ReglasDocumentosPaciente.MaximoBytesPorArchivo))
            return BadRequest(new { mensaje = "Los archivos no pueden estar vacíos ni superar los 20 MB cada uno." });
        if (archivos.Sum(a => a.Length) > ReglasDocumentosPaciente.MaximoBytesTotal)
            return BadRequest(new { mensaje = "Los documentos adjuntos no pueden superar los 30 MB en total." });

        try
        {
            var tipos = tipoDocumentoIds.Distinct().ToList();
            if (await context.TipoDocumentos.CountAsync(t => tipos.Contains(t.Id), cancellationToken) != tipos.Count)
                return BadRequest(new { mensaje = "Uno de los tipos de documento ya no está disponible. Volvé a seleccionarlo." });

            // Los repositorios comparten este contexto: un fallo revierte el alta completa.
            await using var transaccion = await context.Database.BeginTransactionAsync(cancellationToken);
            var id = await pacientes.InsertarPaciente(paciente);
            for (var i = 0; i < archivos.Count; i++)
            {
                var archivo = archivos[i];
                await using var entrada = archivo.OpenReadStream();
                using var memoria = new MemoryStream();
                await entrada.CopyToAsync(memoria, cancellationToken);
                await documentos.InsertarDocumentoConArchivo(id, tipoDocumentoIds[i], archivo.FileName,
                    string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType,
                    memoria.ToArray());
            }
            await transaccion.CommitAsync(cancellationToken);
            return Ok(id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (ApplicationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo registrar el paciente junto con sus documentos.");
            return StatusCode(500, new { mensaje = "No pudimos guardar el paciente y sus documentos. No se guardó el alta. Intentá nuevamente." });
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO;

public sealed class RestablecerContrasenaDTO
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NuevaContrasena), ErrorMessage = "Las contraseñas deben coincidir.")]
    public string Confirmacion { get; set; } = string.Empty;
}

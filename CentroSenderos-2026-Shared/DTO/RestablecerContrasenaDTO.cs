using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.DTO;

public sealed class RestablecerContrasenaDTO
{
    [Required(ErrorMessage = "Seleccioná una cuenta.")]
    public string UsuarioId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá una contraseña nueva.")]
    [DataType(DataType.Password)]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmá la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NuevaContrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    public string Confirmacion { get; set; } = string.Empty;
}

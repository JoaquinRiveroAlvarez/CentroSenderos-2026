using Microsoft.AspNetCore.Identity;

namespace CentroSenderos_2026_Server.Seguridad;

public sealed class ErroresIdentityEnEspanol : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Error("No pudimos completar la operación. Intentá nuevamente.");
    public override IdentityError ConcurrencyFailure() => Error("La cuenta cambió mientras la editabas. Actualizá la página e intentá nuevamente.");
    public override IdentityError PasswordMismatch() => Error("La contraseña actual es incorrecta.");
    public override IdentityError InvalidToken() => Error("El enlace o código ya no es válido. Solicitá uno nuevo.");
    public override IdentityError LoginAlreadyAssociated() => Error("Ese inicio de sesión ya está asociado a otra cuenta.");
    public override IdentityError InvalidUserName(string? userName) => Error("Ingresá un nombre de usuario válido.");
    public override IdentityError InvalidEmail(string? email) => Error("Ingresá un correo electrónico válido.");
    public override IdentityError DuplicateUserName(string userName) => Error("Ese nombre de usuario ya está en uso.");
    public override IdentityError DuplicateEmail(string email) => Error("Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError InvalidRoleName(string? role) => Error("Seleccioná un rol válido.");
    public override IdentityError DuplicateRoleName(string role) => Error("Ya existe un rol con ese nombre.");
    public override IdentityError UserAlreadyHasPassword() => Error("La cuenta ya tiene contraseña. Usá la opción para cambiarla.");
    public override IdentityError UserLockoutNotEnabled() => Error("El bloqueo no está habilitado para esta cuenta.");
    public override IdentityError UserAlreadyInRole(string role) => Error("La cuenta ya tiene ese rol asignado.");
    public override IdentityError UserNotInRole(string role) => Error("La cuenta no tiene ese rol asignado.");
    public override IdentityError RecoveryCodeRedemptionFailed() => Error("El código de recuperación es incorrecto o ya se usó.");
    public override IdentityError PasswordTooShort(int length) => Error($"Usá al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error($"Usá al menos {uniqueChars} caracteres distintos.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error("Agregá un símbolo, como ! o #.");
    public override IdentityError PasswordRequiresDigit() => Error("Agregá un número del 0 al 9.");
    public override IdentityError PasswordRequiresLower() => Error("Agregá una letra minúscula de la a a la z.");
    public override IdentityError PasswordRequiresUpper() => Error("Agregá una letra mayúscula de la A a la Z.");

    private static IdentityError Error(string descripcion, [System.Runtime.CompilerServices.CallerMemberName] string codigo = "") =>
        new() { Code = codigo, Description = descripcion };
}

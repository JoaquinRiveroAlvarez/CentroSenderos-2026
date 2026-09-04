using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_Shared.Validaciones
{
    public sealed class CuitValidoAttribute
        : ValidationAttribute
    {
        public CuitValidoAttribute()
        {
            ErrorMessage =
                "El CUIT ingresado no es válido.";
        }

        public override bool IsValid(object? value)
        {
            /*
             * Los valores vacíos son responsabilidad
             * del atributo [Required].
             */
            if (value is null ||
                string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            return CuitValidador.EsValido(
                value.ToString()
            );
        }
    }
}
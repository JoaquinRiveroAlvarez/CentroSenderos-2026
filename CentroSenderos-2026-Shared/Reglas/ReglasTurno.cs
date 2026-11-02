using System.Globalization;

namespace CentroSenderos_2026_Shared.Reglas
{
    public static class ReglasTurno
    {
        public static bool RequierePacientes(string? tipoTurno)
        {
            return CultureInfo.InvariantCulture.CompareInfo.Compare(
                tipoTurno?.Trim(),
                "Sesión normal",
                CompareOptions.IgnoreCase |
                CompareOptions.IgnoreNonSpace
            ) == 0;
        }
    }
}
using System.Globalization;

namespace CentroSenderos_2026_Shared.Reglas
{
    public static class ReglasTurno
    {
        public const string NombreSesionIndividual = "Sesión individual";
        public const string MensajePacienteIndividual = "La sesión individual requiere exactamente un paciente.";

        // El nombre anterior sigue protegido mientras se aplica la migración de datos.
        public static bool EsSesionIndividual(string? tipoTurno) =>
            Coincide(tipoTurno, NombreSesionIndividual) || Coincide(tipoTurno, "Sesión normal");

        public static bool RequierePacientes(string? tipoTurno) => EsSesionIndividual(tipoTurno);

        public static string? ErrorCantidadPacientes(string? tipoTurno, bool requierePacientes, int cantidad)
        {
            if (EsSesionIndividual(tipoTurno) && cantidad != 1) return MensajePacienteIndividual;
            if (requierePacientes && cantidad == 0) return "Este tipo de turno requiere al menos un paciente.";
            return null;
        }

        private static bool Coincide(string? valor, string esperado) =>
            CultureInfo.InvariantCulture.CompareInfo.Compare(valor?.Trim(), esperado,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}

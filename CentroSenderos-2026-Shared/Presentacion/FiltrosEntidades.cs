using System.Globalization;
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Shared.Presentacion
{
    public static class FiltrosEntidades
    {
        #region Búsqueda de texto

        public static bool ContieneTexto(string? texto, string? busqueda)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                texto ?? string.Empty,
                busqueda?.Trim() ?? string.Empty,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace
            ) >= 0;
        }

        #endregion

        #region Pacientes

        public static bool CoincidePaciente(
            PacienteResumenDTO paciente,
            IReadOnlySet<string> edades,
            IReadOnlySet<string> documentos,
            IReadOnlySet<string> tiposDocumentos,
            DateTime hoy)
        {
            return (edades.Count == 0 || edades.Contains(ObtenerRangoEdad(paciente.FechaNacimiento, hoy)))
                && (documentos.Count == 0 || documentos.Contains(paciente.TiposDocumentos.Count > 0 ? "si" : "no"))
                && (tiposDocumentos.Count == 0 || paciente.TiposDocumentos.Any(tipo => tiposDocumentos.Contains(tipo.Id.ToString())));
        }

        public static string ObtenerRangoEdad(DateTime? nacimiento, DateTime hoy)
        {
            if (!nacimiento.HasValue || nacimiento.Value.Date > hoy.Date)
            {
                return "sin-fecha";
            }

            var edad = hoy.Year - nacimiento.Value.Year;
            if (nacimiento.Value.Date > hoy.Date.AddYears(-edad))
            {
                edad--;
            }

            return edad switch
            {
                <= 5 => "0-5",
                <= 12 => "6-12",
                <= 17 => "13-17",
                <= 59 => "18-59",
                _ => "60+"
            };
        }

        #endregion

        #region Turnos

        public static bool CoincideTurno(
            TurnoListadoDTO turno,
            IReadOnlySet<string> areas,
            IReadOnlySet<string> prestaciones,
            IReadOnlySet<string> estados)
        {
            if (estados.Count > 0 && !estados.Contains(turno.EstadoTurno.ToString()))
            {
                return false;
            }

            if (areas.Count == 0 && prestaciones.Count == 0)
            {
                return true;
            }

            // Área y prestación deben corresponder a la misma atención del turno.
            return turno.ProfesionalPrestaciones.Any(atencion =>
                (areas.Count == 0 || areas.Contains(atencion.TipoAreaId?.ToString() ?? "sin-area"))
                && (prestaciones.Count == 0 || prestaciones.Contains(atencion.TipoPrestacionId.ToString())));
        }

        #endregion
    }
}

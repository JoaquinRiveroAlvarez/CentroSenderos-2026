using System.Globalization;
using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026_Shared.Presentacion
{
    public static class PresentacionProfesional
    {
        public static string ObtenerResumenAreas(IEnumerable<string>? areas)
        {
            var nombres = (areas ?? Array.Empty<string>())
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Select(nombre => nombre.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(nombre => nombre)
                .ToList();

            return nombres.Count == 0
                ? "Sin área asignada"
                : string.Join(", ", nombres);
        }

        public static bool CoincideBusqueda(ProfesionalListadoDTO profesional, string? busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
                return true;

            var texto = string.Join(" ", new[] { profesional.Nombre }
                .Concat(profesional.Areas)
                .Concat(profesional.TipoPrestacionNombres));

            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                texto,
                busqueda.Trim(),
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }
    }
}

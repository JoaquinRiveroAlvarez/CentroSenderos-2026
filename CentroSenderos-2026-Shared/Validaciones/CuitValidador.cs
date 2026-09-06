namespace CentroSenderos_2026_Shared.Validaciones
{
    public static class CuitValidador
    {
        private static readonly int[] Pesos =
        {
            5, 4, 3, 2, 7, 6, 5, 4, 3, 2
        };

        public static bool EsValido(string? cuit)
        {
            if (!TryNormalizar(cuit, out var cuitNormalizado))
            {
                return false;
            }

            if (cuitNormalizado.Length != 11)
            {
                return false;
            }

            var suma = 0;

            for (var i = 0; i < Pesos.Length; i++)
            {
                var digito = cuitNormalizado[i] - '0';

                suma += digito * Pesos[i];
            }

            var resultado = 11 - (suma % 11);

            var digitoCalculado = resultado switch
            {
                11 => 0,
                10 => 9,
                _ => resultado
            };

            var digitoIngresado =
                cuitNormalizado[10] - '0';

            return digitoCalculado == digitoIngresado;
        }

        public static string Normalizar(string? cuit)
        {
            if (!TryNormalizar(cuit, out var resultado))
            {
                return string.Empty;
            }

            return resultado;
        }

        public static string Formatear(string? cuit)
        {
            var cuitNormalizado = Normalizar(cuit);

            if (cuitNormalizado.Length != 11)
            {
                return cuit?.Trim() ?? string.Empty;
            }

            return $"{cuitNormalizado[..2]}-" +
                   $"{cuitNormalizado.Substring(2, 8)}-" +
                   $"{cuitNormalizado[^1]}";
        }

        private static bool TryNormalizar(
            string? cuit,
            out string resultado)
        {
            resultado = string.Empty;

            if (string.IsNullOrWhiteSpace(cuit))
            {
                return false;
            }

            var caracteres = new List<char>();

            foreach (var caracter in cuit.Trim())
            {
                if (char.IsDigit(caracter))
                {
                    caracteres.Add(caracter);
                    continue;
                }

                if (caracter == '-' ||
                    caracter == '.' ||
                    char.IsWhiteSpace(caracter))
                {
                    continue;
                }

                return false;
            }

            resultado = new string(caracteres.ToArray());

            return true;
        }
    }
}
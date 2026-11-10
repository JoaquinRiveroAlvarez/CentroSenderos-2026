using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Servicio.ServiciosHttp
{
    public class HttpServicio : IHttpServicio
    {
        private readonly HttpClient http;
        public HttpServicio(HttpClient http)
        {
            this.http = http;
        }
        public async Task<HttpRespuesta<T>> Get<T>(string url)
        {
            var response = await http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var respuesta = await DesSerializar<T>(response);
                return new HttpRespuesta<T>(respuesta, false, response);
            }
            else
            {
                var errorStr = await ExtraerError(response);
                return new HttpRespuesta<T>(default, true, response, errorStr);
            }
        }
        public async Task<HttpRespuesta<TResp>> Post<T, TResp>(string url, T entidad)
        {
            var JsonAEnviar = JsonSerializer.Serialize(entidad);
            var contenido = new StringContent(JsonAEnviar,
                                              System.Text.Encoding.UTF8,
                                              "application/json");

            var response = await http.PostAsync(url, contenido);
            if (response.IsSuccessStatusCode)
            {
                var respuesta = await DesSerializar<TResp>(response);
                return new HttpRespuesta<TResp>(respuesta, false, response);
            }
            else
            {
                var errorStr = await ExtraerError(response);
                return new HttpRespuesta<TResp>(default, true, response, errorStr);
            }
        }
        public async Task<HttpRespuesta<TResp>> Put<T, TResp>(string url, T entidad)
        {
            var jsonAEnviar = JsonSerializer.Serialize(entidad);
            var contenido = new StringContent(jsonAEnviar,
                                              System.Text.Encoding.UTF8,
                                              "application/json");

            var response = await http.PutAsync(url, contenido);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return new HttpRespuesta<TResp>(default, false, response);
                }

                var respuesta = await DesSerializar<TResp>(response);
                return new HttpRespuesta<TResp>(respuesta, false, response);
            }
            else
            {
                // 👇 leer el body del error
                var contenidoError = await response.Content.ReadAsStringAsync();

                // intentar deserializar si viene en JSON
                TResp? errorObj = default;
                try
                {
                    errorObj = JsonSerializer.Deserialize<TResp>(contenidoError,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // si no es JSON, lo dejamos como default
                }

                var errorStr = LimpiarError(contenidoError);
                return new HttpRespuesta<TResp>(errorObj, true, response, errorStr);
            }
        }

        public async Task<HttpRespuesta<object>> Delete(string url)
        {
            var respuesta = await http.DeleteAsync(url);
            var errorStr = !respuesta.IsSuccessStatusCode ? await ExtraerError(respuesta) : null;
            return new HttpRespuesta<object>(null,
                                             !respuesta.IsSuccessStatusCode,
                                             respuesta,
                                             errorStr);
        }

        public async Task<HttpRespuesta<TResp>> PostMultipart<TResp>(string url, MultipartFormDataContent contenido)
        {
            var response = await http.PostAsync(url, contenido);

            if (response.IsSuccessStatusCode)
            {
                var respuesta = await DesSerializar<TResp>(response);
                return new HttpRespuesta<TResp>(respuesta, false, response);
            }
            else
            {
                var errorStr = await ExtraerError(response);
                return new HttpRespuesta<TResp>(default, true, response, errorStr);
            }
        }

        private static async Task<string?> ExtraerError(HttpResponseMessage response)
        {
            try
            {
                var raw = await response.Content.ReadAsStringAsync();
                return LimpiarError(raw);
            }
            catch
            {
                return null;
            }
        }

        private static string? LimpiarError(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var trimmed = raw.Trim();
            if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private async Task<T?> DesSerializar<T>(HttpResponseMessage response)
        {
            var respStr = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(respStr))
                return default;

            var trimmed = respStr.TrimStart();

            // Comprobar tokens JSON comunes: objeto, array, string, number, true/false, null (minúsculas)
            if (trimmed.StartsWith("{") ||
                trimmed.StartsWith("[") ||
                trimmed.StartsWith("\"") ||
                char.IsDigit(trimmed, 0) ||
                trimmed.StartsWith("-") ||
                trimmed.StartsWith("true", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("false", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("null", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(respStr,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    // registrar ex + respStr para diagnóstico
                    // (aquí puede usar ILogger)
                    throw new InvalidOperationException("Respuesta JSON mal formada", ex);
                }
            }

            // No es JSON: registrar respStr si es necesario y devolver default
            return default;
        }
    }
}
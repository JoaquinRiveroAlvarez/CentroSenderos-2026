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
                return new HttpRespuesta<T>(default, true, response);
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
                return new HttpRespuesta<TResp>(default, true, response);
            }
            //Joaquin
            //var response = await http.postasync(url, contenido);
            //var contenidoerror = await response.content.readasstringasync(); // 👈 línea nueva
            //if (response.issuccessstatuscode)
            //{
            //    var respuesta = await desserializar<tresp>(response);
            //    return new httprespuesta<tresp>(respuesta, false, response);
            //}
            //else
            //{
            //    return new httprespuesta<tresp>(default, true, response);
            //}
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
                return new HttpRespuesta<TResp>(default, true, response);
            }
        }

        public async Task<HttpRespuesta<object>> Delete(string url)
        {
            var respuesta = await http.DeleteAsync(url);
            return new HttpRespuesta<object>(null,
                                             !respuesta.IsSuccessStatusCode,
                                             respuesta);
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

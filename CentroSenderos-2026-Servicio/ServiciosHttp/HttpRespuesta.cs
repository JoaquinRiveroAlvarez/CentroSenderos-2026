using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Servicio.ServiciosHttp
{
    public class HttpRespuesta<T>
    {
        public T? Respuesta { get; }
        public bool Error { get; }
        public HttpResponseMessage HttpResponseMessage { get; set; }
        public string? MensajeErrorServidor { get; set; }

        public HttpRespuesta(T? respuesta,
                             bool error,
                             HttpResponseMessage httpResponseMessage,
                             string? mensajeErrorServidor = null)
        {
            Respuesta = respuesta;
            Error = error;
            HttpResponseMessage = httpResponseMessage;
            MensajeErrorServidor = mensajeErrorServidor;
        }

        public string ObtenerError()
        {
            if (!Error)
            {
                return string.Empty;
            }

            var statuscode = HttpResponseMessage.StatusCode;

            if ((int)statuscode >= 500)
                return "El servidor no pudo completar la solicitud. Intentá nuevamente. Si continúa, avisá a administración.";

            if (!string.IsNullOrWhiteSpace(MensajeErrorServidor))
                return MensajeErrorServidor;

            switch (statuscode)
            {
                case HttpStatusCode.NotFound:
                    return "No hay datos disponibles.";
                case HttpStatusCode.Unauthorized:
                    return "Iniciá sesión para continuar.";
                case HttpStatusCode.Forbidden:
                    return "Tu cuenta no tiene permiso para realizar esta acción.";
                case HttpStatusCode.BadRequest:
                    return "Revisá los datos ingresados e intentá nuevamente.";
                default:
                    return "No pudimos completar la solicitud. Intentá nuevamente.";
            }
        }
    }
}

namespace CentroSenderos_2026_Servicio.ServiciosHttp
{
    public interface IHttpServicio
    {
        Task<HttpRespuesta<object>> Delete(string url);
        Task<HttpRespuesta<T>> Get<T>(string url);
        Task<HttpRespuesta<TResp>> Post<T, TResp>(string url, T entidad);
        Task<HttpRespuesta<TResp>> Put<T, TResp>(string url, T entidad);
    }
}
using System.Reflection;

namespace CentroSenderos_2026.Tests.Infraestructura;

// Sustituye interfaces únicamente en pruebas unitarias.
// Una llamada no configurada falla para evitar respuestas implícitas.
public class RepositorioSimulado : DispatchProxy
{
    public Dictionary<string, Func<object?>> Respuestas { get; } = new();

    public static T Crear<T>(string metodo, Func<object?> respuesta) where T : class
    {
        var interfaz = DispatchProxy.Create<T, RepositorioSimulado>();
        ((RepositorioSimulado)(object)interfaz).Respuestas.Add(metodo, respuesta);
        return interfaz;
    }

    protected override object? Invoke(MethodInfo? metodo, object?[]? argumentos)
    {
        if (metodo is not null && Respuestas.TryGetValue(metodo.Name, out var respuesta))
            return respuesta();
        throw new InvalidOperationException(
            $"Llamada de repositorio no configurada: {metodo?.Name}");
    }
}

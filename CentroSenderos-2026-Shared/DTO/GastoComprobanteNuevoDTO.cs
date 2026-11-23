using System;

namespace CentroSenderos_2026_Shared.DTO;

public sealed class GastoComprobanteNuevoDTO
{
    public const int MaximoPorGuardado = 3;

    public string Nombre { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
}

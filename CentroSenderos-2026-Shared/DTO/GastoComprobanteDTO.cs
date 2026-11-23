using System;

namespace CentroSenderos_2026_Shared.DTO;

public sealed class GastoComprobanteDTO
{
    public const int MaximoBytes = 5 * 1024 * 1024;

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
}
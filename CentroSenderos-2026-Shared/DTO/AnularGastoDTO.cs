using System.Collections.Generic;

namespace CentroSenderos_2026_Shared.DTO;

public enum MotivoReversionReintegro
{
    RegistroErroneo = 1,
    DineroDevuelto = 2
}

public class AnularGastoDTO
{
    public string Motivo { get; set; } = string.Empty;
    public MotivoReversionReintegro? MotivoReversion { get; set; }
    public Dictionary<int, decimal> ReintegrosConfirmados { get; set; } = new();
}

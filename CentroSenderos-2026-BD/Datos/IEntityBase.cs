using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_BD
{
    public interface IEntityBase
    {
        int Id { get; set; }
        string Observacion { get; set; }
        EnumEstadoRegistro EstadoRegistro { get; set; }
    }
}
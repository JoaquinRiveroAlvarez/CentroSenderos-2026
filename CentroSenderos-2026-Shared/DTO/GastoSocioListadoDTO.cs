namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoSocioListadoDTO
    {
        public int SocioId { get; set; }

        public string Profesional { get; set; } = string.Empty;

        public decimal Monto { get; set; }
    }
}
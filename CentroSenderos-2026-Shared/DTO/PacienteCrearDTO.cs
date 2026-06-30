using CentroSenderos_2026_Shared.Enum;
using static System.Net.WebRequestMethods;

namespace CentroSenderos_2026_Shared.DTO
{
    public class PacienteCrearDTO
    {
        public required string Nombre { get; set; }
        public required string DNI { get; set; }
        public required int NumeroAfiliado { get; set; }
        public required string Telefono { get; set; }
        public required string Domicilio { get; set; }

     
        public int TipoObraSocialId { get; set; }
        public int TipoDiagnosticoId { get; set; }


    }
}

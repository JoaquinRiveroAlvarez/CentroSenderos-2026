using System;
using System.Collections.Generic;
using System.Linq;


namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoListadoDTO
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int TipoGastoId { get; set; }

        public string TipoGasto { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public bool TieneHistorial { get; set; }


        public List<GastoSocioListadoDTO> GastoSocios { get; set; } = new();
        public List<GastoRepartoListadoDTO> GastoRepartos { get; set; } = new();

        public List<GastoReintegroListadoDTO> GastoReintegros
        { get; set; } = new();

        public decimal SaldoSocio(int socioId)
        {
            var pagado = GastoSocios
                .Where(aporte => aporte.SocioId == socioId)
                .Sum(aporte => aporte.Monto);

            var parte = GastoRepartos
                .Where(reparto => reparto.SocioId == socioId)
                .Sum(reparto => reparto.Monto);

            var reintegrado = GastoReintegros
                .Where(reintegro => reintegro.SocioPagadorId == socioId)
                .Sum(reintegro => reintegro.Monto);

            var recibido = GastoReintegros
                .Where(reintegro => reintegro.SocioReceptorId == socioId)
                .Sum(reintegro => reintegro.Monto);

            return pagado - parte + reintegrado - recibido;
        }

    }
}
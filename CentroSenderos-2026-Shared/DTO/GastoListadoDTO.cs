using System;
using System.Collections.Generic;
using System.Linq;


namespace CentroSenderos_2026_Shared.DTO
{
    public enum EstadoReintegroGasto
    {
        Pendiente = 1,
        Parcial,
        Saldado,
        NoCorresponde,
        PorRevisar,
        ConSaldoPendiente
    }

    public class GastoListadoDTO
    {
        [System.Text.Json.Serialization.JsonIgnore(
            Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ComprobantesModificados { get; set; }

        public int CantidadComprobantes { get; set; }

        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int TipoGastoId { get; set; }

        public string TipoGasto { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public int CajaId { get; set; }

        public bool TieneHistorial { get; set; }


        public List<GastoSocioListadoDTO> GastoSocios { get; set; } = new();
        public List<GastoRepartoListadoDTO> GastoRepartos { get; set; } = new();

        public List<GastoReintegroListadoDTO> GastoReintegros
        { get; set; } = new();

        public bool PuedeImputarACaja => GastoRepartos.Count == 0 &&
            GastoReintegros.Count == 0 && Monto > 0 &&
            GastoSocios.All(aporte => aporte.Monto > 0) &&
            GastoSocios.Sum(aporte => aporte.Monto) == Monto;

        public bool EsCompatibleConCaja(int cajaId)
        {
            return cajaId > 0 && Monto > 0 &&
                GastoRepartos.Count == 1 &&
                GastoRepartos[0].SocioId == cajaId &&
                GastoRepartos[0].Monto == Monto &&
                GastoSocios.All(aporte => aporte.Monto > 0) &&
                GastoSocios.Sum(aporte => aporte.Monto) == Monto &&
                GastoReintegros.All(reintegro => reintegro.Monto > 0 &&
                    reintegro.SocioPagadorId == cajaId &&
                    reintegro.SocioReceptorId != cajaId) &&
                GastoReintegros.GroupBy(reintegro => reintegro.SocioReceptorId)
                    .All(grupo => grupo.Sum(reintegro => reintegro.Monto) <=
                        GastoSocios.Where(aporte => aporte.SocioId == grupo.Key)
                            .Sum(aporte => aporte.Monto));
        }

        public EstadoReintegroGasto ObtenerEstadoReintegro(int cajaId)
        {
            if (!EsCompatibleConCaja(cajaId))
                return EstadoReintegroGasto.PorRevisar;

            if (SaldoSocio(cajaId) < 0m)
                return GastoReintegros.Count > 0
                    ? EstadoReintegroGasto.Parcial
                    : EstadoReintegroGasto.Pendiente;

            return GastoReintegros.Count > 0
                ? EstadoReintegroGasto.Saldado
                : EstadoReintegroGasto.NoCorresponde;
        }

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
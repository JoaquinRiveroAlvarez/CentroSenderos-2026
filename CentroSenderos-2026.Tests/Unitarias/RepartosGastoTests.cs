using CentroSenderos_2026_Shared.DTO;

namespace CentroSenderos_2026.Tests.Unitarias;

public class RepartosGastoTests
{
    [Fact]
    public void GastoHistoricoSoloAdmiteImputacionCuandoLosPagosCoinciden()
    {
        var gasto = new GastoListadoDTO
        {
            Monto = 100,
            GastoSocios = [new() { SocioId = 2, Monto = 100 }]
        };

        Assert.True(gasto.PuedeImputarACaja);
        Assert.False(gasto.EsCompatibleConCaja(1));

        gasto.GastoSocios[0].Monto = 90;
        Assert.False(gasto.PuedeImputarACaja);
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(80, true)]
    [InlineData(81, false)]
    [InlineData(0, false)]
    public void ReintegrosSeComparanConLosAdelantosDelReceptor(int reintegrado, bool compatible)
    {
        var gasto = new GastoListadoDTO
        {
            Monto = 100,
            GastoSocios = [new() { SocioId = 1, Monto = 20 }, new() { SocioId = 2, Monto = 80 }],
            GastoRepartos = [new() { SocioId = 1, Monto = 100 }],
            GastoReintegros = [new() { SocioPagadorId = 1, SocioReceptorId = 2, Monto = reintegrado }]
        };

        Assert.Equal(compatible, gasto.EsCompatibleConCaja(1));
        Assert.False(gasto.PuedeImputarACaja);
        Assert.Equal(80 - reintegrado, gasto.SaldoSocio(2));
    }
}

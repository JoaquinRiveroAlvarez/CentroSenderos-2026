using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public interface ILiquidacionRepositorio : IRepositorio<Liquidacion>
    {
        Task<List<LiquidacionResumenDTO>> ObtenerResumenLiquidaciones(DateTime? fechaDesde, DateTime? fechaHasta, int? profesionalId);
        Task<LiquidacionDTO?> ObtenerPorIdCompleto(int id);
        Task<LiquidacionDTO?> ObtenerPorNumero(string numeroLiquidacion);
        Task<List<TurnoLiquidacionItemDTO>> ObtenerTurnosPorDetalleId(int detalleLiquidacionId);
        Task<int> GuardarLiquidacionCompleta(GuardarLiquidacionDTO dto);
        Task<bool> ActualizarEstadoPagoLiquidacion(int liquidacionId, EnumEstadoPagoLiquidacion nuevoEstado);
        Task<bool> ActualizarEstadoPagoDetalle(int detalleId, EnumEstadoPagoLiquidacion nuevoEstado);
    }
}

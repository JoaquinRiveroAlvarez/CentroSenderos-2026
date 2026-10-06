using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class ReintegroSocioRepositorio
        : Repositorio<ReintegroSocio>, IReintegroSocioRepositorio
    {
        private readonly ApplicationDbContext context;

        public ReintegroSocioRepositorio(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<int> InsertarReintegroSocio(
            ReintegroSocioDTO dto)
        {
            if (dto == null)
            {
                throw new ApplicationException(
                    "No se recibieron los datos del reintegro.");
            }

            if (dto.Fecha.Date == DateTime.MinValue.Date)
            {
                throw new ApplicationException(
                    "La fecha es obligatoria.");
            }

            if (dto.SocioPagadorId <= 0 ||
                dto.SocioReceptorId <= 0)
            {
                throw new ApplicationException(
                    "Seleccioná el socio que paga y el que recibe.");
            }

            if (dto.SocioPagadorId == dto.SocioReceptorId)
            {
                throw new ApplicationException(
                    "El socio que paga y el que recibe deben ser diferentes.");
            }

            const decimal montoMaximo = 9999999999999999.99m;

            if (dto.Monto <= 0 || dto.Monto > montoMaximo)
            {
                throw new ApplicationException(
                    "El monto del reintegro no es válido.");
            }

            if (decimal.Round(dto.Monto, 2) != dto.Monto)
            {
                throw new ApplicationException(
                    "El monto puede tener como máximo dos decimales.");
            }

            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                throw new ApplicationException(
                    "Seleccioná al menos un gasto.");
            }

            if (dto.Detalles.Any(detalle => detalle == null))
            {
                throw new ApplicationException(
                    "Hay un detalle sin datos.");
            }

            if (dto.Detalles.Any(detalle =>
                detalle.GastoId <= 0 ||
                detalle.Monto <= 0 ||
                detalle.Monto > montoMaximo))
            {
                throw new ApplicationException(
                    "Seleccioná gastos válidos e importes mayores que cero.");
            }

            if (dto.Detalles.Any(detalle =>
                decimal.Round(detalle.Monto, 2) != detalle.Monto))
            {
                throw new ApplicationException(
                    "Los importes aplicados pueden tener como máximo dos decimales.");
            }

            var gastoIds = dto.Detalles
                .Select(detalle => detalle.GastoId)
                .ToList();

            if (gastoIds.Distinct().Count() != gastoIds.Count)
            {
                throw new ApplicationException(
                    "No se puede repetir un gasto en el mismo reintegro.");
            }

            decimal totalDetalles;

            try
            {
                totalDetalles = dto.Detalles.Sum(detalle => detalle.Monto);
            }
            catch (OverflowException)
            {
                throw new ApplicationException(
                    "La suma de los importes supera el valor permitido.");
            }

            if (totalDetalles != dto.Monto)
            {
                throw new ApplicationException(
                    "La suma de los importes aplicados debe coincidir " +
                    "con el monto del reintegro.");
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var socioIds = new[]
            {
                dto.SocioPagadorId,
                dto.SocioReceptorId
            };

            var cantidadSociosValidos = await context.Socios
                .CountAsync(socio =>
                    socioIds.Contains(socio.Id) &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo &&
                    socio.Profesionales != null &&
                    socio.Profesionales.EstadoRegistro ==
                        EnumEstadoRegistro.activo);

            if (cantidadSociosValidos != 2)
            {
                throw new ApplicationException(
                    "Uno de los socios o profesionales no existe o está inactivo.");
            }

            var gastos = await context.Gastos
                .AsNoTracking()
                .Include(gasto => gasto.GastoSocios)
                .Include(gasto => gasto.GastoRepartos)
                .Where(gasto =>
                    gastoIds.Contains(gasto.Id) &&
                    gasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToListAsync();

            if (gastos.Count != gastoIds.Count)
            {
                throw new ApplicationException(
                    "Uno de los gastos no existe o está dado de baja.");
            }

            var reintegrosAnteriores = await context.ReintegrosSociosDetalles
                .AsNoTracking()
                .Where(detalle =>
                    gastoIds.Contains(detalle.GastoId) &&
                    detalle.EstadoRegistro == EnumEstadoRegistro.activo &&
                    detalle.ReintegroSocio != null &&
                    detalle.ReintegroSocio.EstadoRegistro ==
                        EnumEstadoRegistro.activo)
                .Select(detalle => new
                {
                    detalle.GastoId,
                    detalle.Monto,
                    detalle.ReintegroSocio!.SocioPagadorId,
                    detalle.ReintegroSocio!.SocioReceptorId
                })
                .ToListAsync();

            foreach (var detalle in dto.Detalles)
            {
                var gasto = gastos.First(gasto =>
                    gasto.Id == detalle.GastoId);

                if (dto.Fecha.Date < gasto.Fecha.Date)
                {
                    throw new ApplicationException(
                        $"El reintegro no puede tener una fecha anterior " +
                        $"al gasto «{gasto.Descripcion}».");
                }

                var repartos = gasto.GastoRepartos
                    .Where(reparto =>
                        reparto.EstadoRegistro == EnumEstadoRegistro.activo)
                    .ToList();

                if (repartos.Count == 0 ||
                    repartos.Sum(reparto => reparto.Monto) != gasto.Monto)
                {
                    throw new ApplicationException(
                        $"El gasto «{gasto.Descripcion}» " +
                        "no tiene un reparto completo.");
                }

                var aportes = gasto.GastoSocios
                    .Where(aporte =>
                        aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                    .ToList();

                if (aportes.Sum(aporte => aporte.Monto) != gasto.Monto)
                {
                    throw new ApplicationException(
                        $"Los pagos del gasto «{gasto.Descripcion}» " +
                        "no coinciden con su monto.");
                }

                decimal SaldoSocio(int socioId)
                {
                    var pagado = aportes
                        .Where(aporte => aporte.SocioId == socioId)
                        .Sum(aporte => aporte.Monto);

                    var parte = repartos
                        .Where(reparto => reparto.SocioId == socioId)
                        .Sum(reparto => reparto.Monto);

                    var reintegrado = reintegrosAnteriores
                        .Where(reintegro =>
                            reintegro.GastoId == gasto.Id &&
                            reintegro.SocioPagadorId == socioId)
                        .Sum(reintegro => reintegro.Monto);

                    var recibido = reintegrosAnteriores
                        .Where(reintegro =>
                            reintegro.GastoId == gasto.Id &&
                            reintegro.SocioReceptorId == socioId)
                        .Sum(reintegro => reintegro.Monto);

                    return pagado - parte + reintegrado - recibido;
                }

                var saldoPagador = SaldoSocio(dto.SocioPagadorId);
                var saldoReceptor = SaldoSocio(dto.SocioReceptorId);

                if (saldoPagador >= 0)
                {
                    throw new ApplicationException(
                        $"El socio que paga no tiene deuda pendiente " +
                        $"en el gasto «{gasto.Descripcion}».");
                }

                if (saldoReceptor <= 0)
                {
                    throw new ApplicationException(
                        $"El socio que recibe no tiene saldo a favor " +
                        $"en el gasto «{gasto.Descripcion}».");
                }

                if (detalle.Monto > -saldoPagador)
                {
                    throw new ApplicationException(
                        $"El importe aplicado al gasto «{gasto.Descripcion}» " +
                        "supera la deuda pendiente del socio que paga.");
                }

                if (detalle.Monto > saldoReceptor)
                {
                    throw new ApplicationException(
                        $"El importe aplicado al gasto «{gasto.Descripcion}» " +
                        "supera el saldo a favor del socio que recibe.");
                }
            }

            var reintegro = new ReintegroSocio
            {
                Fecha = DateTime.SpecifyKind(
                    dto.Fecha.Date,
                    DateTimeKind.Utc),
                SocioPagadorId = dto.SocioPagadorId,
                SocioReceptorId = dto.SocioReceptorId,
                Monto = dto.Monto,
                Observacion = dto.Observacion?.Trim() ?? string.Empty,
                EstadoRegistro = EnumEstadoRegistro.activo,
                Detalles = dto.Detalles
                    .Select(detalle => new ReintegroSocioDetalle
                    {
                        GastoId = detalle.GastoId,
                        Monto = detalle.Monto,
                        Observacion = string.Empty,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    })
                    .ToList()
            };

            context.ReintegrosSocios.Add(reintegro);

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return reintegro.Id;
        }
    }
}
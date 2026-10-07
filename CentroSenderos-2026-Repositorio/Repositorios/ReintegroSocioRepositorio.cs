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

        public async Task<int> InsertarReintegroSocio(ReintegroSocioDTO dto)
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

            if (dto.SocioPagadorId <= 0 || dto.SocioReceptorId <= 0)
            {
                throw new ApplicationException(
                    "Debe indicarse Caja Senderos y el socio que recibe.");
            }

            if (dto.SocioPagadorId == dto.SocioReceptorId)
            {
                throw new ApplicationException(
                    "Caja Senderos no puede reintegrarse dinero a sí misma.");
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

            var caja = await context.Socios
                .AsNoTracking()
                .SingleOrDefaultAsync(socio =>
                    socio.EsCaja &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo);

            if (caja == null)
            {
                throw new ApplicationException(
                    "Caja Senderos debe estar registrada y activa.");
            }

            if (dto.SocioPagadorId != caja.Id)
            {
                throw new ApplicationException(
                    "Únicamente Caja Senderos puede realizar reintegros.");
            }

            var receptorValido = await context.Socios
                .AnyAsync(socio =>
                    socio.Id == dto.SocioReceptorId &&
                    !socio.EsCaja &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo &&
                    socio.Profesionales != null &&
                    socio.Profesionales.EstadoRegistro ==
                        EnumEstadoRegistro.activo);

            if (!receptorValido)
            {
                throw new ApplicationException(
                    "El socio que recibe no existe o su registro " +
                    "o profesional está inactivo.");
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
                var gasto = gastos.First(g =>
                    g.Id == detalle.GastoId);

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

                if (repartos.Count != 1 ||
                    repartos[0].SocioId != caja.Id ||
                    repartos[0].Monto != gasto.Monto)
                {
                    throw new ApplicationException(
                        $"El gasto «{gasto.Descripcion}» tiene un reparto " +
                        "anterior o incompleto. Debe revisarse para que " +
                        "quede completamente a cargo de Caja Senderos.");
                }

                var aportes = gasto.GastoSocios
                    .Where(aporte =>
                        aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                    .ToList();

                if (aportes.Any(aporte => aporte.Monto <= 0) ||
                    aportes.Sum(aporte => aporte.Monto) != gasto.Monto)
                {
                    throw new ApplicationException(
                        $"Los pagos del gasto «{gasto.Descripcion}» " +
                        "no coinciden con su monto o contienen importes inválidos.");
                }

                var reintegrosDelGasto = reintegrosAnteriores
                    .Where(reintegro => reintegro.GastoId == gasto.Id)
                    .ToList();

                if (reintegrosDelGasto.Any(reintegro =>
                    reintegro.SocioPagadorId != caja.Id ||
                    reintegro.SocioReceptorId == caja.Id))
                {
                    throw new ApplicationException(
                        $"El gasto «{gasto.Descripcion}» tiene reintegros " +
                        "del esquema anterior que deben revisarse.");
                }

                var adelantado = aportes
                    .Where(aporte =>
                        aporte.SocioId == dto.SocioReceptorId)
                    .Sum(aporte => aporte.Monto);

                var recibido = reintegrosDelGasto
                    .Where(reintegro =>
                        reintegro.SocioReceptorId == dto.SocioReceptorId)
                    .Sum(reintegro => reintegro.Monto);

                var saldoPendienteSocio = adelantado - recibido;

                if (saldoPendienteSocio <= 0)
                {
                    throw new ApplicationException(
                        $"El socio no tiene un reintegro pendiente " +
                        $"en el gasto «{gasto.Descripcion}».");
                }

                if (detalle.Monto > saldoPendienteSocio)
                {
                    throw new ApplicationException(
                        $"El importe aplicado al gasto «{gasto.Descripcion}» " +
                        "supera el reintegro pendiente del socio.");
                }

                var pagadoPorCaja = aportes
                    .Where(aporte => aporte.SocioId == caja.Id)
                    .Sum(aporte => aporte.Monto);

                var totalReintegrado = reintegrosDelGasto
                    .Sum(reintegro => reintegro.Monto);

                var saldoPendienteCaja =
                    gasto.Monto - pagadoPorCaja - totalReintegrado;

                if (detalle.Monto > saldoPendienteCaja)
                {
                    throw new ApplicationException(
                        $"El importe aplicado al gasto «{gasto.Descripcion}» " +
                        "supera el total pendiente de reintegrar por Caja Senderos.");
                }
            }

            var reintegro = new ReintegroSocio
            {
                Fecha = DateTime.SpecifyKind(
                    dto.Fecha.Date,
                    DateTimeKind.Utc),
                SocioPagadorId = caja.Id,
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
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class GastoRepositorio
        : Repositorio<Gasto>, IGastoRepositorio
    {
        private readonly ApplicationDbContext context;

        public GastoRepositorio(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<int> InsertarGasto(GastoCrearDTO dto)
        {
            if (dto.Fecha == null ||
                dto.Fecha.Value.Date == DateTime.MinValue.Date)
            {
                throw new ApplicationException(
                    "La fecha es obligatoria.");
            }

            var descripcion = dto.Descripcion?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ApplicationException(
                    "La descripción es obligatoria.");
            }

            if (descripcion.Length > 100)
            {
                throw new ApplicationException(
                    "La descripción no puede exceder los 100 caracteres.");
            }

            if (dto.GastoSocios == null ||
                dto.GastoSocios.Count == 0)
            {
                throw new ApplicationException(
                    "Seleccioná al menos un socio.");
            }

            if (dto.GastoSocios.Any(aporte => aporte == null))
            {
                throw new ApplicationException(
                    "Hay un aporte sin datos.");
            }

            if (dto.GastoSocios.Any(aporte => aporte.Monto <= 0))
            {
                throw new ApplicationException(
                    "El importe de cada socio debe ser mayor que cero.");
            }

            if (dto.GastoSocios.Any(aporte =>
                decimal.Round(aporte.Monto, 2) != aporte.Monto))
            {
                throw new ApplicationException(
                    "Los importes pueden tener como máximo dos decimales.");
            }

            if (dto.GastoSocios.Any(aporte =>
                aporte.Monto > 9999999999999999.99m))
            {
                throw new ApplicationException(
                    "Uno de los aportes supera el importe permitido.");
            }

            var socioIds = dto.GastoSocios
                .Select(aporte => aporte.SocioId)
                .ToList();

            if (socioIds.Distinct().Count() != socioIds.Count)
            {
                throw new ApplicationException(
                    "No se puede agregar el mismo socio más de una vez.");
            }

            var tipoGastoExiste = await context.TipoGastos
                .AnyAsync(tipo =>
                    tipo.Id == dto.TipoGastoId &&
                    tipo.EstadoRegistro == EnumEstadoRegistro.activo);

            if (!tipoGastoExiste)
            {
                throw new ApplicationException(
                    "El concepto seleccionado no existe o está inactivo.");
            }

            var cantidadSociosValidos = await context.Socios
                .CountAsync(socio =>
                    socioIds.Contains(socio.Id) &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo &&
                    socio.Profesionales != null &&
                    socio.Profesionales.EstadoRegistro ==
                        EnumEstadoRegistro.activo);

            if (cantidadSociosValidos != socioIds.Count)
            {
                throw new ApplicationException(
                    "Uno de los socios seleccionados no existe o está inactivo.");
            }

            var aportes = dto.GastoSocios
                .Select(aporte => new GastoSocio
                {
                    SocioId = aporte.SocioId,
                    Monto = aporte.Monto,
                    EstadoRegistro = EnumEstadoRegistro.activo
                })
                .ToList();

            var gasto = new Gasto
            {
                Fecha = DateTime.SpecifyKind(
                    dto.Fecha.Value.Date,
                    DateTimeKind.Utc),
                Descripcion = descripcion,
                TipoGastoId = dto.TipoGastoId,
                Monto = aportes.Sum(aporte => aporte.Monto),
                GastoSocios = aportes,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Gastos.Add(gasto);
            await context.SaveChangesAsync();

            return gasto.Id;
        }

        public async Task<List<GastoListadoDTO>> SelectListaGastos()
        {
            return await context.Gastos
                .AsNoTracking()
                .Where(gasto =>
                    gasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderByDescending(gasto => gasto.Fecha)
                .ThenByDescending(gasto => gasto.Id)
                .Select(gasto => new GastoListadoDTO
                {
                    Id = gasto.Id,
                    Fecha = gasto.Fecha,
                    TipoGastoId = gasto.TipoGastoId,
                    TipoGasto = gasto.TipoGastos != null
                        ? gasto.TipoGastos.Tipo
                        : string.Empty,
                    Descripcion = gasto.Descripcion,
                    Monto = gasto.Monto,
                    GastoSocios = gasto.GastoSocios
                        .Where(aporte =>
                            aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                        .OrderBy(aporte => aporte.SocioId)
                        .Select(aporte => new GastoSocioListadoDTO
                        {
                            SocioId = aporte.SocioId,
                            Profesional = aporte.Socios != null &&
                                          aporte.Socios.Profesionales != null
                                ? aporte.Socios.Profesionales.Nombre
                                : string.Empty,
                            Monto = aporte.Monto
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
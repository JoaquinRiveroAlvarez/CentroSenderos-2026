using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class GastoRepositorio : Repositorio<Gasto>, IGastoRepositorio
    {
        private readonly ApplicationDbContext context;

        public GastoRepositorio(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<int> InsertarGasto(GastoCrearDTO dto)
        {
            if (dto == null)
            {
                throw new ApplicationException(
                    "No se recibieron los datos del gasto.");
            }

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

            if (dto.GastoSocios == null || dto.GastoSocios.Count == 0)
            {
                throw new ApplicationException(
                    "Seleccioná quién pagó el gasto: Caja Senderos o un socio.");
            }

            if (dto.GastoSocios.Any(aporte => aporte == null))
            {
                throw new ApplicationException(
                    "Hay un pago sin datos.");
            }

            if (dto.GastoSocios.Any(aporte =>
                aporte.SocioId <= 0 || aporte.Monto <= 0))
            {
                throw new ApplicationException(
                    "Seleccioná pagadores válidos e importes mayores que cero.");
            }

            if (dto.GastoSocios.Any(aporte =>
                decimal.Round(aporte.Monto, 2) != aporte.Monto))
            {
                throw new ApplicationException(
                    "Los importes pueden tener como máximo dos decimales.");
            }

            const decimal montoMaximo = 9999999999999999.99m;

            if (dto.GastoSocios.Any(aporte => aporte.Monto > montoMaximo))
            {
                throw new ApplicationException(
                    "Uno de los pagos supera el importe permitido.");
            }

            var socioIds = dto.GastoSocios
                .Select(aporte => aporte.SocioId)
                .ToList();

            if (socioIds.Distinct().Count() != socioIds.Count)
            {
                throw new ApplicationException(
                    "No se puede agregar el mismo pagador más de una vez.");
            }

            decimal montoTotal;

            try
            {
                montoTotal = dto.GastoSocios.Sum(aporte => aporte.Monto);
            }
            catch (OverflowException)
            {
                throw new ApplicationException(
                    "El monto total supera el importe permitido.");
            }

            if (montoTotal > montoMaximo)
            {
                throw new ApplicationException(
                    "El monto total supera el importe permitido.");
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var tipoGastoExiste = await context.TipoGastos
                .AnyAsync(tipo =>
                    tipo.Id == dto.TipoGastoId &&
                    tipo.EstadoRegistro == EnumEstadoRegistro.activo);

            if (!tipoGastoExiste)
            {
                throw new ApplicationException(
                    "El tipo de gasto seleccionado no existe o está inactivo.");
            }

            var caja = await context.Socios
                .SingleOrDefaultAsync(socio =>
                    socio.EsCaja &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo);

            if (caja == null)
            {
                throw new ApplicationException(
                    "Caja Senderos debe estar registrada y activa para crear gastos.");
            }

            var cantidadPagadoresValidos = await context.Socios
                .CountAsync(socio =>
                    socioIds.Contains(socio.Id) &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo &&
                    (
                        (socio.EsCaja && socio.ProfesionalId == null) ||
                        (!socio.EsCaja &&
                         socio.Profesionales != null &&
                         socio.Profesionales.EstadoRegistro ==
                            EnumEstadoRegistro.activo)
                    ));

            if (cantidadPagadoresValidos != socioIds.Count)
            {
                throw new ApplicationException(
                    "Uno de los pagadores seleccionados no existe o está inactivo.");
            }

            var aportes = dto.GastoSocios
                .Select(aporte => new GastoSocio
                {
                    SocioId = aporte.SocioId,
                    Monto = aporte.Monto,
                    EstadoRegistro = EnumEstadoRegistro.activo
                })
                .ToList();

            var repartos = new List<GastoReparto>
            {
                new GastoReparto
                {
                    SocioId = caja.Id,
                    Monto = montoTotal,
                    Observacion = string.Empty,
                    EstadoRegistro = EnumEstadoRegistro.activo
                }
            };

            var gasto = new Gasto
            {
                Fecha = DateTime.SpecifyKind(
                    dto.Fecha.Value.Date,
                    DateTimeKind.Utc),
                Descripcion = descripcion,
                TipoGastoId = dto.TipoGastoId,
                Monto = montoTotal,
                GastoSocios = aportes,
                GastoRepartos = repartos,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.Gastos.Add(gasto);

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return gasto.Id;
        }

        public async Task<bool> ActualizarGasto(int id,GastoEditarDTO dto,string usuarioId)
        {
            if (dto == null)
            {
                throw new ApplicationException(
                    "No se recibieron los datos del gasto.");
            }

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                throw new ApplicationException(
                    "No se pudo identificar al usuario.");
            }

            if (dto.Fecha == null ||
                dto.Fecha.Value.Date == DateTime.MinValue.Date)
            {
                throw new ApplicationException(
                    "La fecha es obligatoria.");
            }

            var descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            var motivo = dto.Motivo?.Trim() ?? string.Empty;

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

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new ApplicationException(
                    "Ingresá el motivo de la modificación.");
            }

            if (motivo.Length > 500)
            {
                throw new ApplicationException(
                    "El motivo no puede exceder los 500 caracteres.");
            }

            if (dto.GastoSocios == null || dto.GastoSocios.Count == 0)
            {
                throw new ApplicationException(
                    "Seleccioná quién pagó el gasto: Caja Senderos o un socio.");
            }

            if (dto.GastoSocios.Any(aporte => aporte == null))
            {
                throw new ApplicationException(
                    "Hay un pago sin datos.");
            }

            if (dto.GastoSocios.Any(aporte =>
                aporte.SocioId <= 0 || aporte.Monto <= 0))
            {
                throw new ApplicationException(
                    "Seleccioná pagadores válidos e importes mayores que cero.");
            }

            if (dto.GastoSocios.Any(aporte =>
                decimal.Round(aporte.Monto, 2) != aporte.Monto))
            {
                throw new ApplicationException(
                    "Los importes pueden tener como máximo dos decimales.");
            }

            const decimal montoMaximo = 9999999999999999.99m;

            if (dto.GastoSocios.Any(aporte => aporte.Monto > montoMaximo))
            {
                throw new ApplicationException(
                    "Uno de los pagos supera el importe permitido.");
            }

            var socioIds = dto.GastoSocios
                .Select(aporte => aporte.SocioId)
                .ToList();

            if (socioIds.Distinct().Count() != socioIds.Count)
            {
                throw new ApplicationException(
                    "No se puede agregar el mismo pagador más de una vez.");
            }

            decimal montoTotal;

            try
            {
                montoTotal = dto.GastoSocios.Sum(aporte => aporte.Monto);
            }
            catch (OverflowException)
            {
                throw new ApplicationException(
                    "El monto total supera el importe permitido.");
            }

            if (montoTotal > montoMaximo)
            {
                throw new ApplicationException(
                    "El monto total supera el importe permitido.");
            }

            await using var transaccion =
                await context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var usuario = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                throw new ApplicationException(
                    "El usuario identificado no existe.");
            }

            var gasto = await context.Gastos
                .Include(g => g.TipoGastos)
                .Include(g => g.GastoRepartos)
                    .ThenInclude(reparto => reparto.Socios)
                        .ThenInclude(socio => socio!.Profesionales)
                .Include(g => g.GastoSocios)
                    .ThenInclude(aporte => aporte.Socios)
                        .ThenInclude(socio => socio!.Profesionales)
                .FirstOrDefaultAsync(g =>
                    g.Id == id &&
                    g.EstadoRegistro == EnumEstadoRegistro.activo);

            if (gasto == null)
                return false;

            var tieneReintegros = await context.ReintegrosSociosDetalles
                .AnyAsync(detalle =>
                    detalle.GastoId == id &&
                    detalle.EstadoRegistro == EnumEstadoRegistro.activo &&
                    detalle.ReintegroSocio != null &&
                    detalle.ReintegroSocio.EstadoRegistro ==
                        EnumEstadoRegistro.activo);

            var tipoGasto = await context.TipoGastos
                .FirstOrDefaultAsync(tipo => tipo.Id == dto.TipoGastoId);

            if (tipoGasto == null ||
                (tipoGasto.EstadoRegistro != EnumEstadoRegistro.activo &&
                 dto.TipoGastoId != gasto.TipoGastoId))
            {
                throw new ApplicationException(
                    "El tipo de gasto seleccionado no existe o está inactivo.");
            }

            var aportesActuales = gasto.GastoSocios
                .Where(aporte =>
                    aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToList();

            if (tieneReintegros)
            {
                var aportesSinCambios =
                    aportesActuales.Count == dto.GastoSocios.Count &&
                    aportesActuales.All(actual =>
                        dto.GastoSocios.Any(nuevo =>
                            nuevo.SocioId == actual.SocioId &&
                            nuevo.Monto == actual.Monto));

                if (dto.Fecha.Value.Date != gasto.Fecha.Date ||
                    montoTotal != gasto.Monto ||
                    !aportesSinCambios ||
                    dto.CompletarReparto ||
                    (dto.SocioRepartoIds != null &&
                     dto.SocioRepartoIds.Count > 0))
                {
                    throw new ApplicationException(
                        "Este gasto tiene reintegros registrados. Solo se pueden " +
                        "modificar la descripción y el tipo de gasto; la fecha, " +
                        "el monto, los pagos y el reparto están protegidos.");
                }

                var versionAnterior = JsonSerializer.Serialize(
                    ObtenerVersionGasto(gasto));

                gasto.Descripcion = descripcion;
                gasto.TipoGastoId = tipoGasto.Id;
                gasto.TipoGastos = tipoGasto;

                var versionNueva = JsonSerializer.Serialize(
                    ObtenerVersionGasto(gasto));

                if (versionAnterior == versionNueva)
                {
                    throw new ApplicationException(
                        "No hay cambios en el gasto para guardar.");
                }

                context.GastoHistoriales.Add(new GastoHistorial
                {
                    GastoId = gasto.Id,
                    UsuarioId = usuario.Id,
                    NombreUsuario = usuario.UserName ?? usuario.Id,
                    FechaCambio = DateTime.UtcNow,
                    Motivo = motivo,
                    DatosAnteriores = versionAnterior,
                    DatosNuevos = versionNueva
                });

                await context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return true;
            }

            var socioIdsActuales = aportesActuales
                .Select(aporte => aporte.SocioId)
                .ToList();

            var sociosSeleccionados = await context.Socios
                .Include(socio => socio.Profesionales)
                .Where(socio => socioIds.Contains(socio.Id))
                .ToListAsync();

            if (sociosSeleccionados.Count != socioIds.Count)
            {
                throw new ApplicationException(
                    "Uno de los pagadores seleccionados no existe.");
            }

            var nuevoSocioInvalido = sociosSeleccionados.Any(socio =>
                !socioIdsActuales.Contains(socio.Id) &&
                (
                    socio.EstadoRegistro != EnumEstadoRegistro.activo ||
                    (socio.EsCaja
                        ? socio.ProfesionalId != null
                        : socio.Profesionales == null ||
                          socio.Profesionales.EstadoRegistro !=
                              EnumEstadoRegistro.activo)
                ));

            if (nuevoSocioInvalido)
            {
                throw new ApplicationException(
                    "Uno de los nuevos pagadores no existe o está inactivo.");
            }

            var caja = await context.Socios
                .SingleOrDefaultAsync(socio =>
                    socio.EsCaja &&
                    socio.EstadoRegistro == EnumEstadoRegistro.activo);

            if (caja == null)
            {
                throw new ApplicationException(
                    "Caja Senderos debe estar registrada y activa.");
            }

            var repartosOriginales = gasto.GastoRepartos
                .Where(reparto =>
                    reparto.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToList();

            if (repartosOriginales.Count != 1 ||
                repartosOriginales[0].SocioId != caja.Id ||
                repartosOriginales[0].Monto != gasto.Monto)
            {
                throw new ApplicationException(
                    "Este gasto tiene un reparto anterior o incompleto. " +
                    "Debe revisarse antes de modificar sus pagos o importes.");
            }

            if (dto.CompletarReparto ||
                (dto.SocioRepartoIds != null &&
                 dto.SocioRepartoIds.Count > 0))
            {
                throw new ApplicationException(
                    "El gasto está a cargo de Caja Senderos. " +
                    "No corresponde seleccionar socios para repartirlo.");
            }

            var datosAnteriores = JsonSerializer.Serialize(
                ObtenerVersionGasto(gasto));

            foreach (var aporteActual in aportesActuales)
            {
                if (!socioIds.Contains(aporteActual.SocioId))
                {
                    aporteActual.EstadoRegistro =
                        EnumEstadoRegistro.borrado;
                }
            }

            foreach (var aporteNuevo in dto.GastoSocios)
            {
                var aporteActual = aportesActuales
                    .FirstOrDefault(aporte =>
                        aporte.SocioId == aporteNuevo.SocioId);

                if (aporteActual != null)
                {
                    aporteActual.Monto = aporteNuevo.Monto;
                }
                else
                {
                    var socio = sociosSeleccionados
                        .First(s => s.Id == aporteNuevo.SocioId);

                    gasto.GastoSocios.Add(new GastoSocio
                    {
                        GastoId = gasto.Id,
                        SocioId = aporteNuevo.SocioId,
                        Socios = socio,
                        Monto = aporteNuevo.Monto,
                        EstadoRegistro = EnumEstadoRegistro.activo
                    });
                }
            }

            gasto.Fecha = DateTime.SpecifyKind(
                dto.Fecha.Value.Date,
                DateTimeKind.Utc);

            gasto.TipoGastoId = tipoGasto.Id;
            gasto.TipoGastos = tipoGasto;
            gasto.Descripcion = descripcion;
            gasto.Monto = montoTotal;

            repartosOriginales[0].Monto = montoTotal;

            var datosNuevos = JsonSerializer.Serialize(
                ObtenerVersionGasto(gasto));

            if (datosAnteriores == datosNuevos)
            {
                throw new ApplicationException(
                    "No hay cambios en el gasto para guardar.");
            }

            context.GastoHistoriales.Add(new GastoHistorial
            {
                GastoId = gasto.Id,
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.UserName ?? usuario.Id,
                FechaCambio = DateTime.UtcNow,
                Motivo = motivo,
                DatosAnteriores = datosAnteriores,
                DatosNuevos = datosNuevos
            });

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return true;
        }

        private static GastoListadoDTO ObtenerVersionGasto(Gasto gasto)
        {
            return new GastoListadoDTO
            {
                Id = gasto.Id,
                Fecha = gasto.Fecha,
                TipoGastoId = gasto.TipoGastoId,
                TipoGasto = gasto.TipoGastos?.Tipo ?? string.Empty,
                Descripcion = gasto.Descripcion,
                Monto = gasto.Monto,

                GastoSocios = gasto.GastoSocios
                    .Where(aporte =>
                        aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                    .OrderBy(aporte => aporte.SocioId)
                    .Select(aporte => new GastoSocioListadoDTO
                    {
                        SocioId = aporte.SocioId,
                        Profesional = aporte.Socios != null
                            ? aporte.Socios.EsCaja
                                ? "Caja Senderos"
                                : aporte.Socios.Profesionales != null
                                    ? aporte.Socios.Profesionales.Nombre
                                    : string.Empty
                            : string.Empty,
                        Monto = aporte.Monto
                    })
                    .ToList(),

                GastoRepartos = gasto.GastoRepartos
                    .Where(reparto =>
                        reparto.EstadoRegistro == EnumEstadoRegistro.activo)
                    .OrderBy(reparto => reparto.SocioId)
                    .Select(reparto => new GastoRepartoListadoDTO
                    {
                        SocioId = reparto.SocioId,
                        Profesional = reparto.Socios != null
                            ? reparto.Socios.EsCaja
                                ? "Caja Senderos"
                                : reparto.Socios.Profesionales != null
                                    ? reparto.Socios.Profesionales.Nombre
                                    : string.Empty
                            : string.Empty,
                        Monto = reparto.Monto
                    })
                    .ToList()
            };
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

                    GastoReintegros = gasto.ReintegroSocioDetalles
                        .Where(detalle =>
                            detalle.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            detalle.ReintegroSocio != null &&
                            detalle.ReintegroSocio.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .Select(detalle => new GastoReintegroListadoDTO
                        {
                            ReintegroSocioId = detalle.ReintegroSocioId,
                            Fecha = detalle.ReintegroSocio!.Fecha,
                            SocioPagadorId =
                                detalle.ReintegroSocio!.SocioPagadorId,
                            SocioReceptorId =
                                detalle.ReintegroSocio!.SocioReceptorId,
                            Monto = detalle.Monto
                        })
                        .ToList(),

                    TieneHistorial = context.GastoHistoriales
                        .Any(historial => historial.GastoId == gasto.Id),

                    GastoSocios = gasto.GastoSocios
                        .Where(aporte =>
                            aporte.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .OrderBy(aporte => aporte.SocioId)
                        .Select(aporte => new GastoSocioListadoDTO
                        {
                            SocioId = aporte.SocioId,
                            Profesional = aporte.Socios != null
                                ? aporte.Socios.EsCaja
                                    ? "Caja Senderos"
                                    : aporte.Socios.Profesionales != null
                                        ? aporte.Socios.Profesionales.Nombre
                                        : string.Empty
                                : string.Empty,
                            Monto = aporte.Monto
                        })
                        .ToList(),

                    GastoRepartos = gasto.GastoRepartos
                        .Where(reparto =>
                            reparto.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .OrderBy(reparto => reparto.SocioId)
                        .Select(reparto => new GastoRepartoListadoDTO
                        {
                            SocioId = reparto.SocioId,
                            Profesional = reparto.Socios != null
                                ? reparto.Socios.EsCaja
                                    ? "Caja Senderos"
                                    : reparto.Socios.Profesionales != null
                                        ? reparto.Socios.Profesionales.Nombre
                                        : string.Empty
                                : string.Empty,
                            Monto = reparto.Monto
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<GastoCrearDTO?> SelectGastoPorId(int id)
        {
            return await context.Gastos
                .AsNoTracking()
                .Where(gasto =>
                    gasto.Id == id &&
                    gasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(gasto => new GastoCrearDTO
                {
                    Fecha = gasto.Fecha,
                    TipoGastoId = gasto.TipoGastoId,
                    Descripcion = gasto.Descripcion,

                    GastoSocios = gasto.GastoSocios
                        .Where(aporte =>
                            aporte.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .OrderBy(aporte => aporte.SocioId)
                        .Select(aporte => new GastoSocioDTO
                        {
                            SocioId = aporte.SocioId,
                            Monto = aporte.Monto
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<GastoHistorialDTO>?> SelectHistorialGasto(int gastoId)
        {
            var existe = await context.Gastos
                .AsNoTracking()
                .AnyAsync(gasto => gasto.Id == gastoId);

            if (!existe)
                return null;

            var registros = await context.GastoHistoriales
                .AsNoTracking()
                .Where(historial => historial.GastoId == gastoId)
                .OrderByDescending(historial => historial.FechaCambio)
                .ThenByDescending(historial => historial.Id)
                .Select(historial => new
                {
                    historial.Id,
                    historial.GastoId,
                    historial.NombreUsuario,
                    historial.FechaCambio,
                    historial.Motivo,
                    historial.DatosAnteriores,
                    historial.DatosNuevos
                })
                .ToListAsync();

            return registros
                .Select(registro => new GastoHistorialDTO
                {
                    Id = registro.Id,
                    GastoId = registro.GastoId,
                    NombreUsuario = registro.NombreUsuario,
                    FechaCambio = registro.FechaCambio,
                    Motivo = registro.Motivo,

                    DatosAnteriores =
                        JsonSerializer.Deserialize<GastoListadoDTO>(
                            registro.DatosAnteriores)
                        ?? throw new InvalidOperationException(
                            "No se pudo interpretar la versión anterior del gasto."),

                    DatosNuevos =
                        JsonSerializer.Deserialize<GastoListadoDTO>(
                            registro.DatosNuevos)
                        ?? throw new InvalidOperationException(
                            "No se pudo interpretar la versión nueva del gasto.")
                })
                .ToList();
        }

        public async Task<GastoListadoDTO?> SelectDetalleGasto(int id)
        {
            return await context.Gastos
                .AsNoTracking()
                .Where(gasto =>
                    gasto.Id == id &&
                    gasto.EstadoRegistro == EnumEstadoRegistro.activo)
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

                    GastoReintegros = gasto.ReintegroSocioDetalles
                        .Where(detalle =>
                            detalle.EstadoRegistro ==
                                EnumEstadoRegistro.activo &&
                            detalle.ReintegroSocio != null &&
                            detalle.ReintegroSocio.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .Select(detalle => new GastoReintegroListadoDTO
                        {
                            ReintegroSocioId = detalle.ReintegroSocioId,
                            Fecha = detalle.ReintegroSocio!.Fecha,
                            SocioPagadorId =
                                detalle.ReintegroSocio!.SocioPagadorId,
                            SocioReceptorId =
                                detalle.ReintegroSocio!.SocioReceptorId,
                            Monto = detalle.Monto
                        })
                        .ToList(),

                    TieneHistorial = context.GastoHistoriales
                        .Any(historial => historial.GastoId == gasto.Id),

                    GastoSocios = gasto.GastoSocios
                        .Where(aporte =>
                            aporte.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .OrderBy(aporte => aporte.SocioId)
                        .Select(aporte => new GastoSocioListadoDTO
                        {
                            SocioId = aporte.SocioId,
                            Profesional = aporte.Socios != null
                                ? aporte.Socios.EsCaja
                                    ? "Caja Senderos"
                                    : aporte.Socios.Profesionales != null
                                        ? aporte.Socios.Profesionales.Nombre
                                        : string.Empty
                                : string.Empty,
                            Monto = aporte.Monto
                        })
                        .ToList(),

                    GastoRepartos = gasto.GastoRepartos
                        .Where(reparto =>
                            reparto.EstadoRegistro ==
                                EnumEstadoRegistro.activo)
                        .OrderBy(reparto => reparto.SocioId)
                        .Select(reparto => new GastoRepartoListadoDTO
                        {
                            SocioId = reparto.SocioId,
                            Profesional = reparto.Socios != null
                                ? reparto.Socios.EsCaja
                                    ? "Caja Senderos"
                                    : reparto.Socios.Profesionales != null
                                        ? reparto.Socios.Profesionales.Nombre
                                        : string.Empty
                                : string.Empty,
                            Monto = reparto.Monto
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
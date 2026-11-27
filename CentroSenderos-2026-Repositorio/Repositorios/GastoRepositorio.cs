using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
            AgregarComprobantes(gasto, dto.ComprobantesNuevos);

            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return gasto.Id;
        }

        private List<string> AgregarComprobantes(Gasto gasto, List<GastoComprobanteNuevoDTO>? archivos)
        {
            if (archivos is null || archivos.Count == 0)
                return new List<string>();

            if (archivos.Count > GastoComprobanteNuevoDTO.MaximoPorGuardado)
                throw new ApplicationException("Podés adjuntar hasta 3 comprobantes nuevos por guardado.");

            var nombres = new List<string>(archivos.Count);
            foreach (var archivo in archivos)
            {
                if (archivo?.Contenido is not { Length: > 0 and <= GastoComprobanteDTO.MaximoBytes } contenido)
                    throw new ApplicationException("Cada comprobante debe tener contenido y pesar hasta 5 MB.");

                var nombre = Path.GetFileName(
                    (archivo.Nombre ?? string.Empty).Replace('\\', '/'));
                if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 180 || nombre.Any(char.IsControl))
                    throw new ApplicationException("El nombre de uno de los comprobantes no es válido.");

                var tipo = ObtenerTipoComprobante(Path.GetExtension(nombre), contenido);
                if (tipo is null)
                    throw new ApplicationException("Los comprobantes deben ser PDF, JPG o PNG válidos.");

                context.GastoComprobantes.Add(new GastoComprobante
                {
                    Gasto = gasto,
                    Nombre = nombre,
                    TipoContenido = tipo,
                    Contenido = contenido,
                    TamanoBytes = contenido.Length,
                    FechaCarga = DateTime.UtcNow
                });
                nombres.Add(nombre);
            }

            return nombres;
        }

        private static string? ObtenerTipoComprobante(string extension, ReadOnlySpan<byte> contenido) => extension.ToLowerInvariant() switch
        {
            ".pdf" when contenido.StartsWith("%PDF-"u8) => "application/pdf",
            ".jpg" or ".jpeg" when contenido.StartsWith(new byte[] { 0xff, 0xd8, 0xff }) => "image/jpeg",
            ".png" when contenido.StartsWith(
                new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }) => "image/png",
            _ => null
        };

        public async Task<bool> ActualizarGasto(int id, GastoEditarDTO dto, string usuarioId)
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

            var comprobanteIds = dto.ComprobanteIdsParaQuitar?
                .Distinct().ToArray() ?? Array.Empty<int>();
            var comprobantesQuitados = new List<string>();

            if (comprobanteIds.Length > 0)
            {
                var comprobantesParaQuitar = context.GastoComprobantes
                    .Where(archivo => archivo.GastoId == gasto.Id &&
                                     comprobanteIds.Contains(archivo.Id));

                comprobantesQuitados = await comprobantesParaQuitar
                    .OrderBy(archivo => archivo.Id)
                    .Select(archivo => archivo.Nombre)
                    .ToListAsync();

                if (comprobantesQuitados.Count > 0)
                    await comprobantesParaQuitar.ExecuteDeleteAsync();
            }

            var comprobantesAgregados = AgregarComprobantes(gasto, dto.ComprobantesNuevos);
            var cambioComprobantes = comprobantesAgregados.Count > 0 || comprobantesQuitados.Count > 0;

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

            var repartosOriginales = gasto.GastoRepartos
                .Where(reparto => reparto.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToList();
            var requiereRevision = repartosOriginales.Count != 1 ||
                repartosOriginales[0].Socios?.EsCaja != true ||
                repartosOriginales[0].Socios?.EstadoRegistro != EnumEstadoRegistro.activo ||
                repartosOriginales[0].Monto != gasto.Monto || gasto.Monto <= 0 ||
                aportesActuales.Any(aporte => aporte.Monto <= 0) ||
                aportesActuales.Sum(aporte => aporte.Monto) != gasto.Monto;

            if (tieneReintegros || requiereRevision)
            {
                var aportesSinCambios =
                    dto.GastoSocios != null &&
                    aportesActuales.Count == dto.GastoSocios.Count &&
                    aportesActuales.All(actual =>
                        dto.GastoSocios.Any(nuevo =>
                            nuevo != null && nuevo.SocioId == actual.SocioId &&
                            nuevo.Monto == actual.Monto));

                if (dto.Fecha.Value.Date != gasto.Fecha.Date ||
                    !aportesSinCambios ||
                    dto.CompletarReparto ||
                    (dto.SocioRepartoIds != null &&
                     dto.SocioRepartoIds.Count > 0))
                {
                    throw new ApplicationException(
                        "Este gasto tiene reintegros o datos por revisar. Solo se pueden " +
                        "modificar la descripción y el tipo de gasto; la fecha, " +
                        "el monto, los pagos y el reparto están protegidos.");
                }

                var versionAnterior = SerializarVersionHistorial(gasto, comprobantesQuitados);

                gasto.Descripcion = descripcion;
                gasto.TipoGastoId = tipoGasto.Id;
                gasto.TipoGastos = tipoGasto;

                if (dto.ImputarACaja)
                {
                    var cajaActiva = await context.Socios.SingleOrDefaultAsync(socio =>
                        socio.EsCaja && socio.EstadoRegistro == EnumEstadoRegistro.activo);
                    if (cajaActiva == null || tieneReintegros ||
                        !ObtenerVersionGasto(gasto).PuedeImputarACaja)
                    {
                        throw new ApplicationException(
                            "Solo se puede imputar a Caja un gasto sin reparto ni reintegros y con pagos consistentes.");
                    }

                    gasto.GastoRepartos.Add(new GastoReparto
                    {
                        SocioId = cajaActiva.Id,
                        Socios = cajaActiva,
                        EstadoRegistro = EnumEstadoRegistro.activo,
                        Monto = gasto.Monto
                    });
                }

                var versionNueva = SerializarVersionHistorial(gasto, comprobantesAgregados);

                if (versionAnterior == versionNueva && !cambioComprobantes)
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

            if (dto.ImputarACaja || dto.CompletarReparto ||
                (dto.SocioRepartoIds != null &&
                 dto.SocioRepartoIds.Count > 0))
            {
                throw new ApplicationException(
                    "El gasto está a cargo de Caja Senderos. " +
                    "No corresponde seleccionar socios para repartirlo.");
            }

            var datosAnteriores = SerializarVersionHistorial(gasto, comprobantesQuitados);

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

            var datosNuevos = SerializarVersionHistorial(gasto, comprobantesAgregados);

            if (datosAnteriores == datosNuevos && !cambioComprobantes)
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

        private static string SerializarVersionHistorial(Gasto gasto, List<string> comprobantes)
        {
            var version = ObtenerVersionGasto(gasto);
            if (comprobantes.Count > 0)
                version.ComprobantesModificados = comprobantes;

            return JsonSerializer.Serialize(version);
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

        public async Task<string?> AnularGasto(int id, AnularGastoDTO solicitud, string usuarioId)
        {
            var motivo = solicitud.Motivo?.Trim() ?? string.Empty;
            if (motivo.Length is 0 or > 500)
                throw new ApplicationException("Ingresá un motivo de hasta 500 caracteres.");

            await using var transaccion = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var gasto = await context.Gastos.SingleOrDefaultAsync(g => g.Id == id &&
                (g.EstadoRegistro == EnumEstadoRegistro.activo || g.EstadoRegistro == EnumEstadoRegistro.anulado));
            if (gasto is null)
                throw new ApplicationException("El gasto no está disponible.");
            if (gasto.EstadoRegistro == EnumEstadoRegistro.anulado)
                return null;

            var usuario = await context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == usuarioId)
                ?? throw new ApplicationException("No se pudo identificar a la persona que solicitó la anulación.");
            var version = await SelectDetalleGasto(id)
                ?? throw new ApplicationException("No se pudo recuperar el detalle del gasto.");
            var anterior = JsonSerializer.Serialize(version);
            var detalles = await context.ReintegrosSociosDetalles
                .Where(d => d.GastoId == id && d.EstadoRegistro == EnumEstadoRegistro.activo &&
                    d.ReintegroSocio!.EstadoRegistro == EnumEstadoRegistro.activo)
                .ToListAsync();
            var reintegros = detalles.GroupBy(d => d.ReintegroSocioId)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(d => d.Monto));
            var confirmados = solicitud.ReintegrosConfirmados ?? new();
            var coinciden = confirmados.Count == reintegros.Count &&
                reintegros.All(r => confirmados.TryGetValue(r.Key, out var monto) && monto == r.Value);
            var bloqueado = !coinciden || (reintegros.Count > 0 &&
                solicitud.MotivoReversion is not (MotivoReversionReintegro.RegistroErroneo or MotivoReversionReintegro.DineroDevuelto));
            var fecha = DateTime.UtcNow;
            var nombre = usuario.UserName ?? usuario.Email ?? usuario.Id;
            if (bloqueado)
            {
                version.AnulacionBloqueada = true;
                version.ReintegrosQueBloqueanAnulacion = reintegros.Keys.Order().ToList();
            }
            else
            {
                gasto.EstadoRegistro = EnumEstadoRegistro.anulado;
                version.Anulado = true;
                version.FechaAnulacion = fecha;
                version.UsuarioAnulacion = nombre;
                version.MotivoAnulacion = motivo;
                if (reintegros.Count > 0)
                {
                    foreach (var detalle in detalles)
                        detalle.EstadoRegistro = EnumEstadoRegistro.anulado;

                    version.MotivoReversion = solicitud.MotivoReversion;
                    version.ReintegrosRevertidos = reintegros;
                    version.GastoReintegros.Clear();
                }
            }

            context.GastoHistoriales.Add(new GastoHistorial
            {
                GastoId = id,
                UsuarioId = usuario.Id,
                NombreUsuario = nombre,
                FechaCambio = fecha,
                Motivo = motivo,
                DatosAnteriores = anterior,
                DatosNuevos = JsonSerializer.Serialize(version)
            });
            await context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return bloqueado
                ? "Revisá los reintegros vigentes y confirmá que se registraron por error o que el dinero ya fue devuelto."
                : null;
        }

        private IQueryable<Gasto> GastosActivos() =>
            context.Gastos.AsNoTracking()
                .Where(gasto => gasto.EstadoRegistro == EnumEstadoRegistro.activo);

        public Task<List<GastoListadoDTO>> SelectListaGastos() =>
            ProyectarGastos(GastosActivos()
                .OrderByDescending(gasto => gasto.Fecha)
                .ThenByDescending(gasto => gasto.Id)).ToListAsync();

        private IQueryable<GastoListadoDTO> ProyectarGastos(IQueryable<Gasto> consulta) =>
            consulta.Select(gasto => new GastoListadoDTO
            {
                Id = gasto.Id,
                Anulado = gasto.EstadoRegistro == EnumEstadoRegistro.anulado,
                CantidadComprobantes = context.GastoComprobantes.Count(
                        comprobante => comprobante.GastoId == gasto.Id),
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
            });


        private Task<int> ObtenerCajaId() => context.Socios
            .Where(socio => socio.EsCaja && socio.EstadoRegistro == EnumEstadoRegistro.activo)
            .Select(socio => socio.Id)
            .SingleOrDefaultAsync();

        public async Task<GastosPaginaDTO> ConsultarGastos(FiltroGastosDTO filtro)
        {
            const int cantidadPorPagina = 25;
            var resultado = new GastosPaginaDTO { CajaId = await ObtenerCajaId() };
            var porEstado = filtro.Anulados
                ? context.Gastos.AsNoTracking().Where(gasto => gasto.EstadoRegistro == EnumEstadoRegistro.anulado)
                : filtro.IncluirAnulados
                    ? context.Gastos.AsNoTracking().Where(gasto =>
                        gasto.EstadoRegistro == EnumEstadoRegistro.activo || gasto.EstadoRegistro == EnumEstadoRegistro.anulado)
                    : GastosActivos();
            resultado.Tipos = await context.TipoGastos.AsNoTracking()
                .Where(tipo => tipo.EstadoRegistro == EnumEstadoRegistro.activo ||
                    porEstado.Any(gasto => gasto.TipoGastoId == tipo.Id))
                .OrderBy(tipo => tipo.Tipo)
                .Select(tipo => new TipoListadoDTO { Id = tipo.Id, Tipo = tipo.Tipo })
                .ToListAsync();

            resultado.Pagadores = await porEstado.SelectMany(gasto => gasto.GastoSocios)
                .Where(aporte => aporte.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(aporte => new GastoSocioListadoDTO
                {
                    SocioId = aporte.SocioId,
                    Profesional = aporte.Socios!.EsCaja
                        ? "Caja Senderos" : aporte.Socios.Profesionales!.Nombre
                }).Distinct().OrderBy(aporte => aporte.Profesional).ToListAsync();

            if (filtro.Desde?.Date > filtro.Hasta?.Date)
                return resultado;

            var consulta = porEstado;
            if (filtro.Desde.HasValue)
            {
                var desde = DateTime.SpecifyKind(filtro.Desde.Value.Date, DateTimeKind.Utc);
                consulta = consulta.Where(gasto => gasto.Fecha >= desde);
            }

            if (filtro.Hasta.HasValue && filtro.Hasta.Value.Date < DateTime.MaxValue.Date)
            {
                var hasta = DateTime.SpecifyKind(filtro.Hasta.Value.Date.AddDays(1), DateTimeKind.Utc);
                consulta = consulta.Where(gasto => gasto.Fecha < hasta);
            }

            if (filtro.TipoId > 0)
                consulta = consulta.Where(gasto => gasto.TipoGastoId == filtro.TipoId);

            if (filtro.PagadorId > 0)
                consulta = consulta.Where(gasto => gasto.GastoSocios.Any(aporte =>
                    aporte.EstadoRegistro == EnumEstadoRegistro.activo && aporte.SocioId == filtro.PagadorId));

            consulta = filtro.EstadoComprobante switch
            {
                EstadoComprobanteGasto.ConComprobante => consulta.Where(gasto =>
                    context.GastoComprobantes.Any(comprobante => comprobante.GastoId == gasto.Id)),

                EstadoComprobanteGasto.SinComprobante => consulta.Where(gasto =>
                    !context.GastoComprobantes.Any(comprobante => comprobante.GastoId == gasto.Id)),

                _ => consulta
            };

            if (!string.IsNullOrWhiteSpace(filtro.Texto))
            {
                var literal = filtro.Texto.Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");
                var patron = "%" + literal + "%";
                consulta = consulta.Where(gasto => EF.Functions.ILike(gasto.Descripcion, patron, "!") ||
                    EF.Functions.ILike(gasto.TipoGastos!.Tipo, patron, "!") ||
                    gasto.GastoSocios.Any(aporte => aporte.EstadoRegistro == EnumEstadoRegistro.activo &&
                        EF.Functions.ILike(aporte.Socios!.EsCaja ? "Caja Senderos" :
                            aporte.Socios.Profesionales!.Nombre, patron, "!")));
            }

            consulta = filtro.Descendente
                ? consulta.OrderByDescending(gasto => gasto.Fecha).ThenByDescending(gasto => gasto.Id)
                : consulta.OrderBy(gasto => gasto.Fecha).ThenBy(gasto => gasto.Id);
            var gastos = await ProyectarGastos(consulta).ToListAsync();
            if (filtro.Anulados)
            {
                resultado.Cantidad = gastos.Count;
                resultado.Paginas = Math.Max(1, (int)Math.Ceiling(gastos.Count / (decimal)cantidadPorPagina));
                resultado.Pagina = Math.Clamp(filtro.Pagina, 1, resultado.Paginas);
                resultado.Gastos = gastos.Skip((resultado.Pagina - 1) * cantidadPorPagina)
                    .Take(cantidadPorPagina).ToList();
                return resultado;
            }

            var activos = gastos.Where(gasto => !gasto.Anulado).ToList();
            var validos = activos.Where(gasto => gasto.EsCompatibleConCaja(resultado.CajaId)).ToList();
            var pendientes = validos.Where(gasto => gasto.SaldoSocio(resultado.CajaId) < 0).ToList();
            var idsValidos = validos.Select(gasto => gasto.Id).ToHashSet();
            resultado.Pendientes = pendientes.Count;
            resultado.PorRevisar = activos.Count - validos.Count;
            resultado.Saldados = validos.Count(gasto =>
                gasto.SaldoSocio(resultado.CajaId) == 0 && gasto.GastoReintegros.Count > 0);
            resultado.TotalSeleccion = activos.Sum(gasto => gasto.Monto);
            resultado.Reintegrado = validos.Sum(gasto => gasto.GastoReintegros.Sum(reintegro => reintegro.Monto));
            resultado.PagadoCaja = validos.Sum(gasto => gasto.GastoSocios
                .Where(aporte => aporte.SocioId == resultado.CajaId).Sum(aporte => aporte.Monto));
            resultado.Prioritarios = pendientes.OrderBy(gasto => gasto.Fecha)
                .ThenBy(gasto => gasto.Id).Take(3).ToList();

            var recibidos = validos.SelectMany(gasto => gasto.GastoReintegros)
                .GroupBy(reintegro => reintegro.SocioReceptorId)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(reintegro => reintegro.Monto));
            resultado.Socios = validos.SelectMany(gasto => gasto.GastoSocios)
                .Where(aporte => aporte.SocioId != resultado.CajaId)
                .GroupBy(aporte => aporte.SocioId)
                .Select(grupo => new SaldoGastoSocioDTO
                {
                    Id = grupo.Key,
                    Nombre = grupo.First().Profesional,
                    Adelantado = grupo.Sum(aporte => aporte.Monto),
                    Recibido = recibidos.GetValueOrDefault(grupo.Key)
                }).OrderByDescending(socio => socio.Pendiente).ThenBy(socio => socio.Nombre).ToList();
            resultado.Adelantado = resultado.Socios.Sum(socio => socio.Adelantado);

            var filtrados = filtro.EstadoReintegro switch
            {
                EstadoReintegroGasto.ConSaldoPendiente => pendientes,
                { } estado => activos.Where(gasto => gasto.ObtenerEstadoReintegro(resultado.CajaId) == estado).ToList(),
                null when filtro.PorRevisar => activos.Where(gasto => !idsValidos.Contains(gasto.Id)).ToList(),
                null when filtro.Pendientes => pendientes,
                _ => gastos
            };

            if (filtro.SocioPendienteId > 0)
            {
                filtrados = filtrados.Where(gasto => idsValidos.Contains(gasto.Id)
                    && filtro.SocioPendienteId != resultado.CajaId
                    && gasto.SaldoSocio(filtro.SocioPendienteId) > 0m).ToList();
            }
            resultado.Cantidad = filtrados.Count;
            resultado.Total = filtrados.Where(gasto => !gasto.Anulado).Sum(gasto => gasto.Monto);
            resultado.TotalesPorMesYTipo = filtrados.Where(gasto => !gasto.Anulado)
                .GroupBy(gasto => new { gasto.Fecha.Year, gasto.Fecha.Month, gasto.TipoGastoId, gasto.TipoGasto })
                .OrderByDescending(grupo => grupo.Key.Year)
                .ThenByDescending(grupo => grupo.Key.Month)
                .ThenBy(grupo => grupo.Key.TipoGasto)
                .Select(grupo => new TotalGastosPorMesYTipoDTO
                {
                    Anio = grupo.Key.Year,
                    Mes = grupo.Key.Month,
                    TipoGasto = grupo.Key.TipoGasto ?? string.Empty,
                    Total = grupo.Sum(gasto => gasto.Monto)
                }).ToList();
            resultado.Paginas = Math.Max(1, (int)Math.Ceiling(filtrados.Count / (decimal)cantidadPorPagina));
            resultado.Pagina = Math.Clamp(filtro.Pagina, 1, resultado.Paginas);
            resultado.Gastos = filtrados.Skip((resultado.Pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina).ToList();
            return resultado;
        }

        public async Task<List<GastoListadoDTO>> ConsultarGastosReintegrables(int socioId)
        {
            var cajaId = await ObtenerCajaId();
            if (socioId <= 0 || socioId == cajaId || cajaId == 0)
                return new();

            var candidatos = await ProyectarGastos(GastosActivos()
                .Where(gasto => gasto.GastoSocios.Any(aporte =>
                    aporte.EstadoRegistro == EnumEstadoRegistro.activo &&
                    aporte.SocioId == socioId && aporte.Monto > 0)))
                .ToListAsync();
            return candidatos.Where(gasto => gasto.EsCompatibleConCaja(cajaId) &&
                gasto.SaldoSocio(socioId) > 0).ToList();
        }

        public Task<List<GastoListadoDTO>> BuscarPosiblesDuplicados(DateTime fecha, int tipoGastoId, decimal monto)
        {
            var dia = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);

            return GastosActivos()
                .Where(gasto => gasto.Fecha == dia &&
                    gasto.TipoGastoId == tipoGastoId &&
                    gasto.Monto == monto)
                .OrderByDescending(gasto => gasto.Id)
                .Take(5)
                .Select(gasto => new GastoListadoDTO
                {
                    Id = gasto.Id,
                    Descripcion = gasto.Descripcion
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
            var cajaId = await ObtenerCajaId();

            var detalle = await context.Gastos
                .AsNoTracking()
                .Where(gasto => gasto.Id == id &&
                    (gasto.EstadoRegistro == EnumEstadoRegistro.activo || gasto.EstadoRegistro == EnumEstadoRegistro.anulado))
                .Select(gasto => new GastoListadoDTO
                {
                    Id = gasto.Id,
                    CajaId = cajaId,
                    Anulado = gasto.EstadoRegistro == EnumEstadoRegistro.anulado,
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
                        .OrderBy(aporte => aporte.Id)
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
                        .OrderBy(reparto => reparto.Id)
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

            if (detalle is { Anulado: true })
            {
                var evento = await context.GastoHistoriales.AsNoTracking()
                    .Where(h => h.GastoId == id)
                    .OrderByDescending(h => h.FechaCambio).ThenByDescending(h => h.Id)
                    .Select(h => new { h.FechaCambio, h.NombreUsuario, h.Motivo, h.DatosNuevos })
                    .FirstOrDefaultAsync();
                detalle.FechaAnulacion = evento?.FechaCambio;
                detalle.UsuarioAnulacion = evento?.NombreUsuario;
                detalle.MotivoAnulacion = evento?.Motivo;
                if (evento is not null && JsonSerializer.Deserialize<GastoListadoDTO>(evento.DatosNuevos) is { } version)
                {
                    detalle.ReintegrosRevertidos = version.ReintegrosRevertidos;
                    detalle.MotivoReversion = version.MotivoReversion;
                }
            }

            return detalle;
        }
    }
}
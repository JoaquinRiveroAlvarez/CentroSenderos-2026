using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoGastoRepositorio
        : Repositorio<TipoGasto>, ITipoGastoRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoGastoRepositorio(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<TipoDTO?> SelectPorId(int id)
        {
            return await context.TipoGastos
                .Where(tipoGasto =>
                    tipoGasto.Id == id &&
                    tipoGasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(tipoGasto => new TipoDTO
                {
                    Id = tipoGasto.Id,
                    Tipo = tipoGasto.Tipo,
                    Descripcion = tipoGasto.Descripcion,
                    EstadoRegistro = tipoGasto.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoListadoDTO?> SelectByTipoGasto(string tipo)
        {
            var nombre = tipo.Trim().ToLower();

            return await context.TipoGastos
                .Where(tipoGasto =>
                    tipoGasto.EstadoRegistro == EnumEstadoRegistro.activo &&
                    tipoGasto.Tipo.ToLower() == nombre)
                .Select(tipoGasto => new TipoListadoDTO
                {
                    Id = tipoGasto.Id,
                    Tipo = tipoGasto.Tipo,
                    Descripcion = tipoGasto.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoListadoDTO>> SelectListaTipoGasto()
        {
            return await context.TipoGastos
                .Where(tipoGasto =>
                    tipoGasto.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(tipoGasto => tipoGasto.Tipo)
                .Select(tipoGasto => new TipoListadoDTO
                {
                    Id = tipoGasto.Id,
                    Tipo = tipoGasto.Tipo,
                    Descripcion = tipoGasto.Descripcion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTipoGasto(TipoDTO dto)
        {
            var nombre = Normalizar(dto.Tipo);
            var descripcion = Normalizar(dto.Descripcion);
            Validar(nombre, descripcion);

            var existente = await context.TipoGastos
                .FirstOrDefaultAsync(tipoGasto =>
                    tipoGasto.Tipo.ToLower() == nombre.ToLower());

            if (existente is not null)
            {
                if (existente.EstadoRegistro == EnumEstadoRegistro.activo)
                {
                    throw new ApplicationException(
                        "Ya existe un tipo de gasto con ese nombre.");
                }

                // Reactiva un tipo eliminado en lugar de duplicarlo.
                existente.Tipo = nombre;
                existente.Descripcion = descripcion;
                existente.EstadoRegistro = EnumEstadoRegistro.activo;

                await context.SaveChangesAsync();
                return existente.Id;
            }

            var entidad = new TipoGasto
            {
                Tipo = nombre,
                Descripcion = descripcion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoGastos.Add(entidad);
            await context.SaveChangesAsync();

            return entidad.Id;
        }

        public async Task<bool> ActualizarTipoGasto(int id, TipoDTO dto)
        {
            var entidad = await context.TipoGastos
                .FirstOrDefaultAsync(tipoGasto =>
                    tipoGasto.Id == id &&
                    tipoGasto.EstadoRegistro == EnumEstadoRegistro.activo);

            if (entidad is null)
                return false;

            var nombre = Normalizar(dto.Tipo);
            var descripcion = Normalizar(dto.Descripcion);
            Validar(nombre, descripcion);

            var nombreOcupado = await context.TipoGastos
                .AnyAsync(tipoGasto =>
                    tipoGasto.Id != id &&
                    tipoGasto.Tipo.ToLower() == nombre.ToLower());

            if (nombreOcupado)
            {
                throw new ApplicationException(
                    "Ya existe un tipo de gasto con ese nombre.");
            }

            entidad.Tipo = nombre;
            entidad.Descripcion = descripcion;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTipoGasto(int id)
        {
            var entidad = await context.TipoGastos
                .FirstOrDefaultAsync(tipoGasto =>
                    tipoGasto.Id == id &&
                    tipoGasto.EstadoRegistro == EnumEstadoRegistro.activo);

            if (entidad is null)
                return false;

            // Conserva los tipos asociados a gastos registrados.
            var estaEnUso = await context.Gastos
                .AnyAsync(gasto => gasto.TipoGastoId == id);

            if (estaEnUso)
            {
                throw new ApplicationException(
                    "No se puede eliminar el tipo porque está asociado a uno o más gastos.");
            }

            entidad.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();

            return true;
        }

        private static string Normalizar(string? valor) =>
            valor?.Trim() ?? string.Empty;

        private static void Validar(string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ApplicationException(
                    "El nombre del tipo de gasto es obligatorio.");
            }

            if (nombre.Length > 50)
            {
                throw new ApplicationException(
                    "El nombre del tipo de gasto no puede superar los 50 caracteres.");
            }

            if (descripcion.Length > 50)
            {
                throw new ApplicationException(
                    "La descripción no puede superar los 50 caracteres.");
            }
        }
    }
}
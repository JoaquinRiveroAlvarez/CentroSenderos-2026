using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class TipoAreaRepositorio
        : Repositorio<TipoArea>, ITipoAreaRepositorio
    {
        private readonly ApplicationDbContext context;

        public TipoAreaRepositorio(ApplicationDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<TipoDTO?> SelectPorId(int id)
        {
            return await context.TipoAreas
                .Where(area =>
                    area.Id == id &&
                    area.EstadoRegistro == EnumEstadoRegistro.activo)
                .Select(area => new TipoDTO
                {
                    Id = area.Id,
                    Tipo = area.Tipo,
                    Descripcion = area.Descripcion,
                    EstadoRegistro = area.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TipoListadoDTO?> SelectByTipoArea(string tipo)
        {
            var nombre = tipo.Trim().ToLower();

            return await context.TipoAreas
                .Where(area =>
                    area.EstadoRegistro == EnumEstadoRegistro.activo &&
                    area.Tipo.ToLower() == nombre)
                .Select(area => new TipoListadoDTO
                {
                    Id = area.Id,
                    Tipo = area.Tipo,
                    Descripcion = area.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TipoListadoDTO>> SelectListaTipoArea()
        {
            return await context.TipoAreas
                .Where(area =>
                    area.EstadoRegistro == EnumEstadoRegistro.activo)
                .OrderBy(area => area.Tipo)
                .Select(area => new TipoListadoDTO
                {
                    Id = area.Id,
                    Tipo = area.Tipo,
                    Descripcion = area.Descripcion
                })
                .ToListAsync();
        }

        public async Task<int> InsertarTipoArea(TipoDTO dto)
        {
            var nombre = Normalizar(dto.Tipo);
            var descripcion = Normalizar(dto.Descripcion);
            Validar(nombre, descripcion);

            var existente = await context.TipoAreas
                .FirstOrDefaultAsync(area =>
                    area.Tipo.ToLower() == nombre.ToLower());

            if (existente is not null)
            {
                if (existente.EstadoRegistro == EnumEstadoRegistro.activo)
                    throw new ApplicationException(
                        "Ya existe un tipo de área con ese nombre.");

                // Permite volver a utilizar un tipo eliminado sin duplicarlo.
                existente.Tipo = nombre;
                existente.Descripcion = descripcion;
                existente.EstadoRegistro = EnumEstadoRegistro.activo;

                await context.SaveChangesAsync();
                return existente.Id;
            }

            var entidad = new TipoArea
            {
                Tipo = nombre,
                Descripcion = descripcion,
                EstadoRegistro = EnumEstadoRegistro.activo
            };

            context.TipoAreas.Add(entidad);
            await context.SaveChangesAsync();

            return entidad.Id;
        }

        public async Task<bool> ActualizarTipoArea(int id, TipoDTO dto)
        {
            var entidad = await context.TipoAreas
                .FirstOrDefaultAsync(area =>
                    area.Id == id &&
                    area.EstadoRegistro == EnumEstadoRegistro.activo);

            if (entidad is null)
                return false;

            var nombre = Normalizar(dto.Tipo);
            var descripcion = Normalizar(dto.Descripcion);
            Validar(nombre, descripcion);

            var nombreOcupado = await context.TipoAreas
                .AnyAsync(area =>
                    area.Id != id &&
                    area.Tipo.ToLower() == nombre.ToLower());

            if (nombreOcupado)
                throw new ApplicationException(
                    "Ya existe un tipo de área con ese nombre.");

            var nombreAnterior = entidad.Tipo;

            // Mientras Profesional todavía guarda el área como texto,
            // conserva sincronizados los profesionales al renombrarla.
            if (!string.Equals(
                    nombreAnterior,
                    nombre,
                    StringComparison.Ordinal))
            {
                var profesionales = await context.Profesionales
                    .Where(profesional =>
                        profesional.Area.Trim().ToLower() ==
                        nombreAnterior.ToLower())
                    .ToListAsync();

                foreach (var profesional in profesionales)
                    profesional.Area = nombre;
            }

            entidad.Tipo = nombre;
            entidad.Descripcion = descripcion;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTipoArea(int id)
        {
            var entidad = await context.TipoAreas
                .FirstOrDefaultAsync(area =>
                    area.Id == id &&
                    area.EstadoRegistro == EnumEstadoRegistro.activo);

            if (entidad is null)
                return false;

            // Evita quitar del catálogo un área asignada a profesionales.
            var estaEnUso = await context.Profesionales
                .AnyAsync(profesional =>
                    profesional.EstadoRegistro != EnumEstadoRegistro.borrado &&
                    profesional.Area.Trim().ToLower() ==
                    entidad.Tipo.ToLower());

            if (estaEnUso)
                throw new ApplicationException(
                    "No se puede eliminar el área porque está asignada a uno o más profesionales.");

            entidad.EstadoRegistro = EnumEstadoRegistro.borrado;
            await context.SaveChangesAsync();

            return true;
        }

        private static string Normalizar(string? valor) =>
            valor?.Trim() ?? string.Empty;

        private static void Validar(string nombre, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ApplicationException(
                    "El nombre del área es obligatorio.");

            if (nombre.Length > 50)
                throw new ApplicationException(
                    "El nombre del área no puede superar los 50 caracteres.");

            if (descripcion.Length > 50)
                throw new ApplicationException(
                    "La descripción no puede superar los 50 caracteres.");
        }
    }
}
using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class PacienteRepositorio : Repositorio<Paciente>, IPacienteRepositorio
    {
        private readonly ApplicationDbContext context;

        public PacienteRepositorio(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }


        public async Task<PacienteDTO?> SelectPorId(int pacienteId)
        {
            return await context.Pacientes
                .Include(p => p.TipoObraSociales)
                .Include(p => p.TipoDiagnosticos)
                .Where(p => p.Id == pacienteId)
                .Select(p => new PacienteDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,
                    NumeroAfiliado = p.NumeroAfiliado,
                    Telefono = p.Telefono ?? string.Empty,
                    Domicilio = p.Domicilio ?? string.Empty,
                    EstadoRegistro = p.EstadoRegistro,

                    TipoObraSocialId = p.TipoObraSocialId,
                    TipoObraSocialNombre = p.TipoObraSociales!.Tipo,

                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    TipoDiagnosticoNombre = p.TipoDiagnosticos!.Tipo
                })
                .FirstOrDefaultAsync();
        }


        public async Task<List<PacienteResumenDTO>> SelectListaPaciente()
        {
            return await context.Pacientes
                .Where(p => p.EstadoRegistro == EnumEstadoRegistro.activo)
                .Include(p => p.TipoObraSociales)
                .Include(p => p.TipoDiagnosticos)
                .OrderBy(p => p.TipoObraSociales!.Tipo)
                .Select(p => new PacienteResumenDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,
                    NumeroAfiliado = p.NumeroAfiliado,
                    EstadoRegistro = p.EstadoRegistro,

                    TipoObraSocialId = p.TipoObraSocialId,
                    TipoObraSocialNombre = p.TipoObraSociales!.Tipo,

                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    TipoDiagnosticoNombre = p.TipoDiagnosticos!.Tipo
                })
                .ToListAsync();
        }


        public async Task<int> InsertarPaciente(PacienteCrearDTO dto)
        {
            var nombreLimpio = NormalizarTexto(dto.Nombre);
            var dniLimpio = NormalizarDni(dto.DNI);
            var telefonoLimpio = NormalizarTelefono(dto.Telefono);
            var domicilioLimpio = NormalizarDomicilio(dto.Domicilio);


            // Validar relaciones

            var obraSocialExiste = await context.TipoObrasSociales
                .AnyAsync(o => o.Id == dto.TipoObraSocialId);

            if (!obraSocialExiste)
            {
                throw new ApplicationException(
                    "La obra social seleccionada no existe."
                );
            }


            var diagnosticoExiste = await context.TipoDiagnosticos
                .AnyAsync(d => d.Id == dto.TipoDiagnosticoId);

            if (!diagnosticoExiste)
            {
                throw new ApplicationException(
                    "El diagnóstico seleccionado no existe."
                );
            }


            // Validar duplicados

            var dniExiste = await context.Pacientes
                .AnyAsync(p => p.DNI == dniLimpio);

            if (dniExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el DNI '{dniLimpio}'."
                );
            }


            var numeroAfiliadoExiste = await context.Pacientes
                .AnyAsync(p => p.NumeroAfiliado == dto.NumeroAfiliado);

            if (numeroAfiliadoExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el número de afiliado '{dto.NumeroAfiliado}'."
                );
            }


            var paciente = new Paciente
            {
                Nombre = nombreLimpio,
                DNI = dniLimpio,
                NumeroAfiliado = dto.NumeroAfiliado,
                Telefono = telefonoLimpio,
                Domicilio = domicilioLimpio,
                TipoObraSocialId = dto.TipoObraSocialId,
                TipoDiagnosticoId = dto.TipoDiagnosticoId,
                EstadoRegistro = EnumEstadoRegistro.activo
            };


            context.Pacientes.Add(paciente);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("DNI_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el DNI '{dniLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains("NumeroAfiliado_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el número de afiliado '{dto.NumeroAfiliado}'."
                    );
                }

                throw;
            }


            return paciente.Id;
        }


        public async Task<bool> ActualizarPaciente(
            int id,
            PacienteDTO dto)
        {
            var paciente = await context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return false;


            var nombreLimpio = NormalizarTexto(dto.Nombre);
            var dniLimpio = NormalizarDni(dto.DNI);
            var telefonoLimpio = NormalizarTelefono(dto.Telefono ?? "");
            var domicilioLimpio = NormalizarDomicilio(dto.Domicilio ?? "");


            // Validar relaciones

            var obraSocialExiste = await context.TipoObrasSociales
                .AnyAsync(o => o.Id == dto.TipoObraSocialId);

            if (!obraSocialExiste)
            {
                throw new ApplicationException(
                    "La obra social seleccionada no existe."
                );
            }


            var diagnosticoExiste = await context.TipoDiagnosticos
                .AnyAsync(d => d.Id == dto.TipoDiagnosticoId);

            if (!diagnosticoExiste)
            {
                throw new ApplicationException(
                    "El diagnóstico seleccionado no existe."
                );
            }


            // Validar duplicados

            var dniExiste = await context.Pacientes
                .AnyAsync(p =>
                    p.DNI == dniLimpio &&
                    p.Id != id);

            if (dniExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el DNI '{dniLimpio}'."
                );
            }


            var numeroAfiliadoExiste = await context.Pacientes
                .AnyAsync(p =>
                    p.NumeroAfiliado == dto.NumeroAfiliado &&
                    p.Id != id);

            if (numeroAfiliadoExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el número de afiliado '{dto.NumeroAfiliado}'."
                );
            }


            // Actualizar

            paciente.Nombre = nombreLimpio;
            paciente.DNI = dniLimpio;
            paciente.NumeroAfiliado = dto.NumeroAfiliado;
            paciente.Telefono = telefonoLimpio;
            paciente.Domicilio = domicilioLimpio;
            paciente.TipoObraSocialId = dto.TipoObraSocialId;
            paciente.TipoDiagnosticoId = dto.TipoDiagnosticoId;


            try
            {
                context.Pacientes.Update(paciente);

                await context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("DNI_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el DNI '{dniLimpio}'."
                    );
                }

                if (ex.InnerException?.Message.Contains("NumeroAfiliado_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el número de afiliado '{dto.NumeroAfiliado}'."
                    );
                }

                throw;
            }
        }


        public async Task<bool> DeletePaciente(int id)
        {
            var paciente = await context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return false;


            paciente.EstadoRegistro = EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();

            return true;
        }


        private static string NormalizarTexto(string texto)
        {
            var cultura = new System.Globalization.CultureInfo("es-AR");

            texto = texto.Trim().ToLower(cultura);

            return cultura.TextInfo.ToTitleCase(texto);
        }


        private static string NormalizarDni(string dni)
        {
            return dni
                .Trim()
                .Replace(".", "")
                .Replace(" ", "");
        }


        private static string NormalizarTelefono(string telefono)
        {
            return telefono.Trim();
        }


        private static string NormalizarDomicilio(string domicilio)
        {
            return domicilio.Trim();
        }
    }
}
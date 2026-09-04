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

        public PacienteRepositorio(
            ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<PacienteDTO?> SelectPorId(int pacienteId)
        {
            return await context.Pacientes
                .Where(p => p.Id == pacienteId)
                .Select(p => new PacienteDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,
                    FechaNacimiento = p.FechaNacimiento,
                    TieneCud = p.TieneCud,
                    NumeroAfiliado = p.NumeroAfiliado,
                    Telefono = p.Telefono ?? string.Empty,
                    Domicilio = p.Domicilio ?? string.Empty,
                    EstadoRegistro = p.EstadoRegistro,

                    Telefonos = p.Telefonos
                        .Where(t =>
                            t.EstadoRegistro ==
                            EnumEstadoRegistro.activo)
                        .Select(t => new PacienteTelefonoDTO
                        {
                            Id = t.Id,
                            Numero = t.Numero,
                            Etiqueta = t.Etiqueta
                        })
                        .ToList(),

                    TipoObraSocialId = p.TipoObraSocialId,
                    TipoObraSocialNombre =
                        p.TipoObraSociales!.Tipo,

                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    TipoDiagnosticoNombre =
                        p.TipoDiagnosticos!.Tipo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PacienteResumenDTO>> SelectListaPaciente()
        {
            return await context.Pacientes
                .Where(p =>
                    p.EstadoRegistro ==
                    EnumEstadoRegistro.activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new PacienteResumenDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,
                    FechaNacimiento = p.FechaNacimiento,
                    TieneCud = p.TieneCud,
                    NumeroAfiliado = p.NumeroAfiliado,
                    Telefono = p.Telefono ?? string.Empty,
                    EstadoRegistro = p.EstadoRegistro,

                    Telefonos = p.Telefonos
                        .Where(t =>
                            t.EstadoRegistro ==
                            EnumEstadoRegistro.activo)
                        .Select(t => new PacienteTelefonoDTO
                        {
                            Id = t.Id,
                            Numero = t.Numero,
                            Etiqueta = t.Etiqueta
                        })
                        .ToList(),

                    TipoObraSocialId = p.TipoObraSocialId,
                    TipoObraSocialNombre =
                        p.TipoObraSociales!.Tipo,

                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    TipoDiagnosticoNombre =
                        p.TipoDiagnosticos!.Tipo
                })
                .ToListAsync();
        }

        public async Task<int> InsertarPaciente(PacienteCrearDTO dto)
        {
            var nombreLimpio =
                NormalizarTexto(dto.Nombre);

            var dniLimpio =
                NormalizarDni(dto.DNI);

            var telefonoLimpio =
                NormalizarTelefono(dto.Telefono);

            var domicilioLimpio =
                NormalizarDomicilio(dto.Domicilio);

            var telefonosLimpios = dto.Telefonos
                .Where(t =>
                    !string.IsNullOrWhiteSpace(t.Numero) &&
                    !string.IsNullOrWhiteSpace(t.Etiqueta))
                .Select(t => new PacienteTelefono
                {
                    Numero = NormalizarTelefono(t.Numero),
                    Etiqueta = NormalizarTexto(t.Etiqueta),
                    EstadoRegistro =
                        EnumEstadoRegistro.activo
                })
                .ToList();

            // Compatibilidad temporal con el formulario anterior.
            if (telefonosLimpios.Count == 0 &&
                !string.IsNullOrWhiteSpace(dto.Telefono))
            {
                telefonosLimpios.Add(new PacienteTelefono
                {
                    Numero = telefonoLimpio,
                    Etiqueta = "Principal",
                    EstadoRegistro =
                        EnumEstadoRegistro.activo
                });
            }

            if (telefonosLimpios.Count == 0)
            {
                throw new ApplicationException(
                    "Debe registrar al menos un teléfono."
                );
            }

            var obraSocialExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Id == dto.TipoObraSocialId);

            if (!obraSocialExiste)
            {
                throw new ApplicationException(
                    "La obra social seleccionada no existe."
                );
            }

            var diagnosticoExiste =
                await context.TipoDiagnosticos
                    .AnyAsync(d =>
                        d.Id == dto.TipoDiagnosticoId);

            if (!diagnosticoExiste)
            {
                throw new ApplicationException(
                    "El diagnóstico seleccionado no existe."
                );
            }

            var dniExiste = await context.Pacientes
                .AnyAsync(p => p.DNI == dniLimpio);

            if (dniExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el DNI '{dniLimpio}'."
                );
            }

            var ultimoNumeroAfiliado = await context.Pacientes
                .MaxAsync(p => (int?)p.NumeroAfiliado) ?? 0;

            var nuevoNumeroAfiliado =
                ultimoNumeroAfiliado + 1;

            var paciente = new Paciente
            {
                Nombre = nombreLimpio,
                DNI = dniLimpio,
                FechaNacimiento = dto.FechaNacimiento,
                TieneCud = dto.TieneCud,
                NumeroAfiliado = nuevoNumeroAfiliado,

                // Se conserva hasta eliminar la columna anterior.
                Telefono = telefonosLimpios
                    .First()
                    .Numero,

                Telefonos = telefonosLimpios,

                Domicilio = domicilioLimpio,
                TipoObraSocialId = dto.TipoObraSocialId,
                TipoDiagnosticoId = dto.TipoDiagnosticoId,
                EstadoRegistro =
                    EnumEstadoRegistro.activo
            };

            context.Pacientes.Add(paciente);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message
                    .Contains("DNI_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el DNI '{dniLimpio}'."
                    );
                }

                if (ex.InnerException?.Message
                    .Contains("NumeroAfiliado_UQ") == true)
                {
                    throw new ApplicationException(
                        $"No se pudo asignar el número de afiliado '{nuevoNumeroAfiliado}'. Intentá guardar nuevamente."
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
                .Include(p => p.Telefonos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
                return false;

            var nombreLimpio =
                NormalizarTexto(dto.Nombre);

            var dniLimpio =
                NormalizarDni(dto.DNI);

            var domicilioLimpio =
                NormalizarDomicilio(dto.Domicilio ?? "");

            var telefonosLimpios = dto.Telefonos
                .Where(t =>
                    !string.IsNullOrWhiteSpace(t.Numero) &&
                    !string.IsNullOrWhiteSpace(t.Etiqueta))
                .Select(t => new PacienteTelefono
                {
                    Numero = NormalizarTelefono(t.Numero),
                    Etiqueta = NormalizarTexto(t.Etiqueta),
                    PacienteId = id,
                    EstadoRegistro =
                        EnumEstadoRegistro.activo
                })
                .ToList();

            // Compatibilidad temporal con registros anteriores.
            if (telefonosLimpios.Count == 0 &&
                !string.IsNullOrWhiteSpace(dto.Telefono))
            {
                telefonosLimpios.Add(new PacienteTelefono
                {
                    Numero =
                        NormalizarTelefono(dto.Telefono),

                    Etiqueta = "Principal",
                    PacienteId = id,
                    EstadoRegistro =
                        EnumEstadoRegistro.activo
                });
            }

            if (telefonosLimpios.Count == 0)
            {
                throw new ApplicationException(
                    "Debe registrar al menos un teléfono."
                );
            }

            var obraSocialExiste =
                await context.TipoObrasSociales
                    .AnyAsync(o =>
                        o.Id == dto.TipoObraSocialId);

            if (!obraSocialExiste)
            {
                throw new ApplicationException(
                    "La obra social seleccionada no existe."
                );
            }

            var diagnosticoExiste =
                await context.TipoDiagnosticos
                    .AnyAsync(d =>
                        d.Id == dto.TipoDiagnosticoId);

            if (!diagnosticoExiste)
            {
                throw new ApplicationException(
                    "El diagnóstico seleccionado no existe."
                );
            }

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

            var numeroAfiliadoExiste =
                await context.Pacientes
                    .AnyAsync(p =>
                        p.NumeroAfiliado ==
                        dto.NumeroAfiliado &&
                        p.Id != id);

            if (numeroAfiliadoExiste)
            {
                throw new ApplicationException(
                    $"Ya existe un paciente con el número de afiliado '{dto.NumeroAfiliado}'."
                );
            }

            paciente.Nombre = nombreLimpio;
            paciente.DNI = dniLimpio;
            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.TieneCud = dto.TieneCud;
            paciente.NumeroAfiliado =
                dto.NumeroAfiliado;

            // Se conserva hasta eliminar la columna anterior.
            paciente.Telefono = telefonosLimpios
                .First()
                .Numero;

            /*
             * La edición reemplaza la lista anterior por la lista
             * completa enviada desde el formulario.
             */
            context.PacienteTelefonos.RemoveRange(
                paciente.Telefonos
            );

            paciente.Telefonos = telefonosLimpios;

            paciente.Domicilio = domicilioLimpio;
            paciente.TipoObraSocialId =
                dto.TipoObraSocialId;

            paciente.TipoDiagnosticoId =
                dto.TipoDiagnosticoId;

            try
            {
                context.Pacientes.Update(paciente);

                await context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message
                    .Contains("DNI_UQ") == true)
                {
                    throw new ApplicationException(
                        $"Ya existe un paciente con el DNI '{dniLimpio}'."
                    );
                }

                if (ex.InnerException?.Message
                    .Contains("NumeroAfiliado_UQ") == true)
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

            paciente.EstadoRegistro =
                EnumEstadoRegistro.borrado;

            await context.SaveChangesAsync();

            return true;
        }

        private static string NormalizarTexto(string texto)
        {
            var cultura =
                new System.Globalization.CultureInfo("es-AR");

            texto = texto
                .Trim()
                .ToLower(cultura);

            return cultura.TextInfo.ToTitleCase(texto);
        }

        private static string NormalizarDni(string dni)
        {
            return dni
                .Trim()
                .Replace(".", "")
                .Replace(" ", "");
        }

        private static string NormalizarTelefono(
            string telefono)
        {
            return telefono.Trim();
        }

        private static string NormalizarDomicilio(
            string domicilio)
        {
            return domicilio.Trim();
        }
    }
}
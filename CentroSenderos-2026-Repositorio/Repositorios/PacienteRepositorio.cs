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

        // Obtener lista completa de pacientes
        public async Task<List<PacienteResumenDTO>> SelectListaPaciente()
        {
            return await context.Pacientes
                .Select(p => new PacienteResumenDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,

                    TipoObraSocialId = p.TipoObraSocialId,
                    NumeroAfiliado = p.NumeroAfiliado,
                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    EstadoRegistro = p.EstadoRegistro
                })
                .ToListAsync();
        }

        // Obtener un paciente por Id
        public async Task<PacienteResumenDTO?> SelectPorDni(int pacienteId)
        {
            PacienteResumenDTO? entidad = await context.Pacientes
                .Select(p => new PacienteResumenDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,

                    TipoObraSocialId = p.TipoObraSocialId,
                    NumeroAfiliado = p.NumeroAfiliado,
                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync(x => x.Id == pacienteId);
            return entidad;
        }

        // Insertar paciente con validaciones
        public async Task<int> InsertarPaciente(PacienteCrearDTO dto)
        {

            var paciente = new Paciente
            {
                Nombre = dto.Nombre,
                DNI = dto.DNI,

                TipoObraSocialId = dto.TipoObraSocialId,
                NumeroAfiliado = dto.NumeroAfiliado,
                TipoDiagnosticoId = dto.TipoDiagnosticoId,

                Telefono = dto.Telefono,
                Domicilio = dto.Domicilio,

                EstadoRegistro = EnumEstadoRegistro.EnGrabacion // valor por defecto
            };

            context.Pacientes.Add(paciente);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Paciente_DNI_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un paciente con el DNI '{dto.DNI}'.");
                }
                
                throw;
            }

            return paciente.Id;
        }

        // Actualizar paciente
        public async Task<bool> UpdatePaciente(int id, PacienteDTO dto)
        {
            var paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return false;

      
            paciente.Nombre = dto.Nombre;
            paciente.DNI = dto.DNI;
            paciente.TipoObraSocialId = dto.TipoObraSocialId;
            paciente.NumeroAfiliado = dto.NumeroAfiliado;
            paciente.TipoDiagnosticoId = dto.TipoDiagnosticoId;
            paciente.Telefono = dto.Telefono;
            paciente.EstadoRegistro = dto.EstadoRegistro;

            await context.SaveChangesAsync();
            return true;
        }

        // Eliminar paciente
        public async Task<bool> DeletePaciente(int id)
        {
            var paciente = await context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return false;

            context.Pacientes.Remove(paciente);
            await context.SaveChangesAsync();
            return true;
        }

        public Task<PacienteResumenDTO?> SelectPorId(int pacienteId)
        {
            throw new NotImplementedException();
        }

        

        public Task<bool> ActualizarPaciente(int id, PacienteDTO dto)
        {
            throw new NotImplementedException();
        }
    }
    }


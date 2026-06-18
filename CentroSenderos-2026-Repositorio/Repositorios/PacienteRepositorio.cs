using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using CentroSenderos_2026_Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Modelado2025_1Repositorio.Repositorios;
using System.Linq.Expressions;

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
                    FechaNacimiento = p.FechaNacimiento,
                    TipoObraSocialId = p.TipoObraSocialId,
                    NumeroAfiliado = p.NumeroAfiliado,
                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    ProfesionalId = p.ProfesionalId,
                    EstadoRegistro = p.EstadoRegistro
                })
                .ToListAsync();
        }

        // Obtener un paciente por Id
        public async Task<PacienteResumenDTO?> SelectPorId(int pacienteId)
        {
            return await context.Pacientes
                .Where(p => p.Id == pacienteId)
                .Select(p => new PacienteResumenDTO
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    DNI = p.DNI,
                    FechaNacimiento = p.FechaNacimiento,
                    TipoObraSocialId = p.TipoObraSocialId,
                    NumeroAfiliado = p.NumeroAfiliado,
                    TipoDiagnosticoId = p.TipoDiagnosticoId,
                    ProfesionalId = p.ProfesionalId,
                    EstadoRegistro = p.EstadoRegistro
                })
                .FirstOrDefaultAsync();
        }

        // Insertar paciente con validaciones
        public async Task<int> InsertarPaciente(PacienteCrearDTO dto)
        {
            var historiaClinica = await GenerarCodigoUnico();

            var paciente = new Paciente
            {
                Nombre = dto.Nombre,
                DNI = dto.DNI,
                Genero = dto.Genero,
                FechaNacimiento = dto.FechaNacimiento,
                TipoObraSocialId = dto.TipoObraSocialId,
                NumeroAfiliado = dto.NumeroAfiliado,
                TipoDiagnosticoId = dto.TipoDiagnosticoId,
                ProfesionalId = dto.ProfesionalId,
                HistoriaClinica = historiaClinica,
                TipoDocumentoId = dto.TipoDocumentoId,
                Telefono = dto.Telefono,
                Domicilio = dto.Domicilio,
                CorreoElectronico = dto.CorreoElectronico,
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
                if (ex.InnerException?.Message.Contains("Paciente_Correo_UQ") == true)
                {
                    throw new ApplicationException($"Ya existe un paciente con el correo '{dto.CorreoElectronico}'.");
                }
                throw;
            }

            return paciente.Id;
        }

        // Actualizar paciente
        public async Task<bool> UpdatePaciente(int id, PacienteEditarDTO dto)
        {
            var paciente = await context.Pacientes.FindAsync(id);
            if (paciente == null) return false;

            paciente.Nombre = dto.Nombre;
            paciente.DNI = dto.DNI;
            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.TipoObraSocialId = dto.TipoObraSocialId;
            paciente.NumeroAfiliado = dto.NumeroAfiliado;
            paciente.TipoDiagnosticoId = dto.TipoDiagnosticoId;
            paciente.ProfesionalId = dto.ProfesionalId;
            paciente.EstadoRegistro = dto.EstadoRegistro;

            await context.SaveChangesAsync();
            return true;
        }

        // Eliminar paciente
        public async Task<bool> DeletePaciente(int id)
        {
            var paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return false;

            context.Pacientes.Remove(paciente);
            await context.SaveChangesAsync();
            return true;
        }

        // Generar código único de historia clínica
        private async Task<int> GenerarCodigoUnico()
        {
            var random = new Random();
            int historiaClinica;
            bool existe;

            do
            {
                historiaClinica = random.Next(100000, 999999);
                existe = await context.Pacientes.AnyAsync(e => e.HistoriaClinica == historiaClinica);
            } while (existe);

            return historiaClinica;
        }
    }
}

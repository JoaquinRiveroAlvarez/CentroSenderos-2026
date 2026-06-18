using CentroSenderos_2026_BD;
using CentroSenderos_2026_BD.Datos.Entity;
using CentroSenderos_2026_Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_Repositorio.Repositorios
{
    public class PacienteRepositorio : IPacienteRepositorio
    {
        private readonly ApplicationDbContext context;

        public PacienteRepositorio(ApplicationDbContext context)
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

        // Crear paciente con código único de historia clínica
        public async Task<int> CrearPaciente(PacienteCrearDTO dto)
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
                EstadoRegistro = dto.EstadoRegistro
            };

            context.Pacientes.Add(paciente);
            await context.SaveChangesAsync();

            return paciente.Id;
        }

        // Actualizar paciente
        public async Task<bool> UpdatePacienteActualizar(int id, PacienteEditarDTO dto)
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
        public async Task<bool> Delete(int id)
        {
            var paciente = await context.Pacientes.FindAsync(id);
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

        #region Interfaz implementada
        public Task<bool> Existe(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> Insert(Paciente entidad)
        {
            throw new NotImplementedException();
        }

        public Task<List<Paciente>> Select()
        {
            throw new NotImplementedException();
        }

        public Task<Paciente?> SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(int id, Paciente entidad)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id, Paciente entidad)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

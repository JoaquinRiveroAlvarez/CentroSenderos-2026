using CentroSenderos_2026_BD.Datos;
using CentroSenderos_2026_BD.Datos.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_BD
{
    public class ApplicationDbContext : IdentityDbContext<MiUsuario>
    {
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<TipoConsultorio> TipoConsultorios { get; set; }
        public DbSet<TipoDiagnostico> TipoDiagnosticos { get; set; }

        public DbSet<TipoDocumento> TipoDocumentos { get; set; }
        public DbSet<TipoGasto> TipoGastos { get; set; }
        public DbSet<TipoModalidad> TipoModalidades { get; set; }
        public DbSet<TipoObraSocial> TipoObrasSociales { get; set; }
        public DbSet<TipoPlanilla> TipoPlanillas { get; set; }
        public DbSet<TipoPrestacion> TipoPrestaciones { get; set; }
        public DbSet<TipoTurno> TipoTurnos { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var cascadeFKs = modelBuilder.Model
                .G­etEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Casca­de);
            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restr­ict;
            }


            modelBuilder.Entity<Paciente>()
                    .HasOne(p => p.TipoObraSociales)
                    .WithMany(o => o.Pacientes)
                    .HasForeignKey(p => p.TipoObraSocialId);

            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.TipoDiagnosticos)
                .WithMany(d => d.Pacientes)
                .HasForeignKey(p => p.TipoDiagnosticoId);

        }
    }
}

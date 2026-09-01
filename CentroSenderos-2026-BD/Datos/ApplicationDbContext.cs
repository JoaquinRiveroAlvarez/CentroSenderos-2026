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
        public DbSet<ProfesionalTipoPrestacion> ProfesionalTipoPrestaciones { get; set; }
        public DbSet<TipoConsultorio> TipoConsultorios { get; set; }
        public DbSet<TipoDiagnostico> TipoDiagnosticos { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<TipoDocumento> TipoDocumentos { get; set; }
        public DbSet<TipoGasto> TipoGastos { get; set; }
        public DbSet<TipoModalidad> TipoModalidades { get; set; }
        public DbSet<TipoObraSocial> TipoObrasSociales { get; set; }
        public DbSet<TipoPlanilla> TipoPlanillas { get; set; }
        public DbSet<TipoPrestacion> TipoPrestaciones { get; set; }
        public DbSet<TipoTurno> TipoTurnos { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<TipoObraSocial> TipoObraSociales { get; set; }
        public DbSet<DetalleLiquidacion> DetalleLiquidaciones { get; set; }
        public DbSet<Liquidacion> Liquidaciones { get; set; }
        public DbSet<Gasto> Gastos { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProfesionalTipoPrestacion>()
    .HasIndex(x => new
    {
        x.ProfesionalId,
        x.TipoPrestacionId
    })
    .IsUnique();

            modelBuilder.Entity<ProfesionalTipoPrestacion>()
                .HasOne(x => x.Profesional)
                .WithMany(p => p.ProfesionalTipoPrestaciones)
                .HasForeignKey(x => x.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProfesionalTipoPrestacion>()
                .HasOne(x => x.TipoPrestacion)
                .WithMany(tp => tp.ProfesionalTipoPrestaciones)
                .HasForeignKey(x => x.TipoPrestacionId)
                .OnDelete(DeleteBehavior.Restrict);

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

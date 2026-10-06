using CentroSenderos_2026_BD.Datos;
using CentroSenderos_2026_BD.Datos.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_BD
{
    public class ApplicationDbContext : IdentityDbContext<MiUsuario>
    {
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<PacienteTelefono> PacienteTelefonos { get; set; }
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
        public DbSet<TipoArea> TipoAreas { get; set; }
        public DbSet<TipoPrestacion> TipoPrestaciones { get; set; }
        public DbSet<TipoTurno> TipoTurnos { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<TurnoTipoPrestacion> TurnoTipoPrestaciones { get; set; }
        public DbSet<SerieTurno> SeriesTurnos { get; set; }
        public DbSet<TipoObraSocial> TipoObraSociales { get; set; }
        public DbSet<DetalleLiquidacion> DetalleLiquidaciones { get; set; }
        public DbSet<Liquidacion> Liquidaciones { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<GastoHistorial> GastoHistoriales { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<GastoReparto> GastoRepartos { get; set; }
        public DbSet<ReintegroSocio> ReintegrosSocios { get; set; }
        public DbSet<ReintegroSocioDetalle> ReintegrosSociosDetalles { get; set; }

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

            modelBuilder.Entity<TurnoTipoPrestacion>()
    .HasIndex(relacion => new
    {
        relacion.TurnoId,
        relacion.ProfesionalId
    })
    .IsUnique();


            modelBuilder.Entity<TurnoTipoPrestacion>()
                .HasOne(relacion => relacion.Turnos)
                .WithMany(turno =>
                    turno.TurnoTipoPrestaciones
                )
                .HasForeignKey(relacion =>
                    relacion.TurnoId
                )
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<TurnoTipoPrestacion>()
                .HasOne(relacion =>
                    relacion.Profesionales
                )
                .WithMany()
                .HasForeignKey(relacion =>
                    relacion.ProfesionalId
                )
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<TurnoTipoPrestacion>()
                .HasOne(relacion =>
                    relacion.TipoPrestaciones
                )
                .WithMany(prestacion =>
                    prestacion.TurnoTipoPrestaciones
                )
                .HasForeignKey(relacion =>
                    relacion.TipoPrestacionId
                )
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

            modelBuilder.Entity<PacienteTelefono>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Telefonos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoPaciente>()
                .HasOne(tp => tp.TipoObraSocial)
                .WithMany()
                .HasForeignKey(tp => tp.TipoObraSocialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(turno => turno.SerieTurno)
                .WithMany(serie => serie.Turnos)
                .HasForeignKey(turno => turno.SerieTurnoId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GastoSocio>()
                .Property(aporte => aporte.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<GastoHistorial>()
                .HasOne(historial => historial.Gasto)
                .WithMany()
                .HasForeignKey(historial => historial.GastoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GastoHistorial>()
                .HasOne(historial => historial.Usuario)
                .WithMany()
                .HasForeignKey(historial => historial.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GastoHistorial>()
                .HasIndex(historial => new
                {
                    historial.GastoId,
                    historial.FechaCambio
                });

            modelBuilder.Entity<GastoReparto>()
                .Property(reparto => reparto.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<GastoReparto>()
                .HasIndex(reparto => new
                {
                    reparto.GastoId,
                    reparto.SocioId
                })
                .IsUnique();

            modelBuilder.Entity<GastoReparto>()
                .HasOne(reparto => reparto.Gastos)
                .WithMany(gasto => gasto.GastoRepartos)
                .HasForeignKey(reparto => reparto.GastoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GastoReparto>()
                .HasOne(reparto => reparto.Socios)
                .WithMany(socio => socio.GastoRepartos)
                .HasForeignKey(reparto => reparto.SocioId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ReintegroSocio>()
    .Property(reintegro => reintegro.Monto)
    .HasPrecision(18, 2);

            modelBuilder.Entity<ReintegroSocio>()
                .HasOne(reintegro => reintegro.SocioPagador)
                .WithMany(socio => socio.ReintegrosPagados)
                .HasForeignKey(reintegro => reintegro.SocioPagadorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroSocio>()
                .HasOne(reintegro => reintegro.SocioReceptor)
                .WithMany(socio => socio.ReintegrosRecibidos)
                .HasForeignKey(reintegro => reintegro.SocioReceptorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroSocioDetalle>()
                .Property(detalle => detalle.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ReintegroSocioDetalle>()
                .HasOne(detalle => detalle.ReintegroSocio)
                .WithMany(reintegro => reintegro.Detalles)
                .HasForeignKey(detalle => detalle.ReintegroSocioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroSocioDetalle>()
                .HasOne(detalle => detalle.Gasto)
                .WithMany(gasto => gasto.ReintegroSocioDetalles)
                .HasForeignKey(detalle => detalle.GastoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReintegroSocioDetalle>()
                .HasIndex(detalle => new
                {
                    detalle.ReintegroSocioId,
                    detalle.GastoId
                })
                .IsUnique();
        }
    }
}

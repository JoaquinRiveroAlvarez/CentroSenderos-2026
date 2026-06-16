using CentroSenderos_2026_BD.Datos;
using CentroSenderos_2026_BD.Datos.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroSenderos_2026_BD.Datos.Entity;

namespace CentroSenderos_2026_BD
{
    public class ApplicationDbContext : IdentityDbContext<MiUsuario>
    {
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<TipoObraSocial> TipoObrasSociales { get; set; }
        public DbSet<TipoDiagnostico> TipoDiagnosticos { get; set; }
        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<Profesional> TipoProductos { get; set; }

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
        }
    }
}

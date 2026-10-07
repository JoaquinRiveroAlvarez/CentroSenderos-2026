using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(ProfesionalId), Name = "ProfesionalId_UQ", IsUnique = true)]
    public class Socio : EntityBase
    {
        public int? ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }
        public bool EsCaja { get; set; }
        public List<GastoSocio> GastoSocios { get; set; } = new();
        public List<GastoReparto> GastoRepartos { get; set; } = new();
        public List<ReintegroSocio> ReintegrosPagados { get; set; } = new();
        public List<ReintegroSocio> ReintegrosRecibidos { get; set; } = new();
    }
}
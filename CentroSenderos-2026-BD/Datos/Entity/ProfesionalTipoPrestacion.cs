using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class ProfesionalTipoPrestacion : EntityBase
    {
        [Required, MaxLength(30)]
        public string Matricula { get; set; } = string.Empty;

        [Required]
        public int ProfesionalId { get; set; }

        public Profesional Profesional { get; set; } = null!;

        [Required]
        public int TipoPrestacionId { get; set; }

        public TipoPrestacion TipoPrestacion { get; set; } = null!;
    }
}
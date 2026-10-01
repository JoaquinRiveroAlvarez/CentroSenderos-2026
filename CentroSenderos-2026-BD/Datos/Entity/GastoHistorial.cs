using CentroSenderos_2026_BD;
using System;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class GastoHistorial
    {
        public int Id { get; set; }

        public int GastoId { get; set; }

        public Gasto? Gasto { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public MiUsuario? Usuario { get; set; }

        [Required]
        [MaxLength(256)]
        public string NombreUsuario { get; set; } = string.Empty;

        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        public string DatosAnteriores { get; set; } = string.Empty;

        [Required]
        public string DatosNuevos { get; set; } = string.Empty;
    }
}
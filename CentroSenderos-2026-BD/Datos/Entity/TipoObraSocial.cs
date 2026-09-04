using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(
        nameof(Tipo),
        Name = "TipoObraSocial_Tipo_UQ",
        IsUnique = true
    )]
    [Index(
        nameof(Cuit),
        Name = "TipoObraSocial_Cuit_UQ",
        IsUnique = true
    )]
    public class TipoObraSocial : EntityTipoBase
    {
        /*
         * Es nullable en la base para permitir que las obras sociales
         * existentes sean actualizadas progresivamente.
         */
        [MaxLength(11)]
        public string? Cuit { get; set; }

        public List<Paciente> Pacientes { get; set; } = new();
    }
}
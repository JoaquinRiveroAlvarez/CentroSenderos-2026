using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CentroSenderos_2026_Shared.DTO
{
    public class GastoCrearDTO
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccioná un concepto")]
        public int TipoGastoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100,
            ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná al menos un socio")]
        [MinLength(1,
            ErrorMessage = "Seleccioná al menos un socio")]
        public List<GastoSocioDTO> GastoSocios { get; set; } = new();

        public decimal Monto =>
            GastoSocios?.Sum(aporte => aporte.Monto) ?? 0m;
    }
}
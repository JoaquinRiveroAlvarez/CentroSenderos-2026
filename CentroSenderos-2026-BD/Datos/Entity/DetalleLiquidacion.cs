using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class DetalleLiquidacion : EntityBase
    {
        [Required(ErrorMessage = "La liquidación es obligatoria")]
        public int LiquidacionId { get; set; }
        public Liquidacion? Liquidaciones { get; set; }

        [Required(ErrorMessage = "La modalidad es obligatoria")]
        public int TipoModalidadId { get; set; }
        public TipoModalidad? TipoModalidades { get; set; }

        public int? TipoPrestacionId { get; set; }
        public TipoPrestacion? TipoPrestaciones { get; set; }

        public int? TipoObraSocialId { get; set; }
        public TipoObraSocial? TipoObraSociales { get; set; }

        // Nullable: solo para Particular y Obra Social con paciente; nulo para Módulos, Talleres, Horas
        public int? PacienteId { get; set; }
        public Paciente? Pacientes { get; set; }

        // Precios y cantidades unitarias
        [Precision(18, 2)]
        public decimal MontoUnitario { get; set; } = 0;

        public int Cantidad { get; set; } = 0;

        // Específico para Módulos (puede combinar completos y medios)
        [Precision(18, 2)]
        public decimal? MontoModuloCompleto { get; set; }
        public int? CantidadModuloCompleto { get; set; }

        [Precision(18, 2)]
        public decimal? MontoMedioModulo { get; set; }
        public int? CantidadMedioModulo { get; set; }

        // Totales de la fila
        [Required(ErrorMessage = "El monto facturado es obligatorio")]
        [Precision(18, 2)]
        public decimal MontoFacturacion { get; set; } = 0;

        [Required(ErrorMessage = "El porcentaje de retención es obligatorio")]
        [Precision(18, 2)]
        public decimal Porcentaje { get; set; } = 0;

        [Required(ErrorMessage = "La retención es obligatoria")]
        [Precision(18, 2)]
        public decimal Retencion { get; set; } = 0;

        [Required(ErrorMessage = "El saldo es obligatorio")]
        [Precision(18, 2)]
        public decimal Saldo { get; set; } = 0;

        // Estado del detalle individual (Pendiente, EnProceso, Parcial, Pagado)
        [Required(ErrorMessage = "El estado del pago es obligatorio")]
        public EnumEstadoPagoLiquidacion EstadoPago { get; set; } = EnumEstadoPagoLiquidacion.Pendiente;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Lista de turnos que componen este detalle específico (Sesiones 1, 2, 3...)
        public List<Turno> Turnos { get; set; } = new();
    }
}
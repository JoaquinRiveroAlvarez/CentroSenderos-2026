using CentroSenderos_2026_Shared.Enum;

namespace CentroSenderos_2026_Shared.Recurrencias
{
    public static class CalculadorRecurrenciaTurno
    {
        private const int MaximoTurnosPorSerie = 500;

        public static List<DateTime> CalcularFechas(
            DateTime fechaInicio,
            EnumFrecuenciaRecurrenciaTurno frecuencia,
            DateTime? fechaHasta,
            int intervalo,
            EnumUnidadRecurrenciaTurno? unidadPersonalizada
        )
        {
            var fechaInicial = fechaInicio.Date;

            if (frecuencia ==
                EnumFrecuenciaRecurrenciaTurno.noRepite)
            {
                return new List<DateTime>
                {
                    fechaInicial
                };
            }

            if (!fechaHasta.HasValue)
            {
                throw new ApplicationException(
                    "Debe indicar hasta qué fecha se repetirá el turno."
                );
            }

            var fechaFinal = fechaHasta.Value.Date;

            if (fechaFinal < fechaInicial)
            {
                throw new ApplicationException(
                    "La fecha final de la repetición no puede ser anterior al primer turno."
                );
            }

            if (frecuencia ==
                    EnumFrecuenciaRecurrenciaTurno.personalizado
                && intervalo <= 0)
            {
                throw new ApplicationException(
                    "El intervalo personalizado debe ser mayor que cero."
                );
            }

            if (frecuencia ==
                    EnumFrecuenciaRecurrenciaTurno.personalizado
                && !unidadPersonalizada.HasValue)
            {
                throw new ApplicationException(
                    "Debe seleccionar una unidad para la repetición personalizada."
                );
            }

            var fechas = new List<DateTime>();
            var numeroRepeticion = 0;

            while (true)
            {
                var fechaCalculada = CalcularFecha(
                    fechaInicial,
                    frecuencia,
                    numeroRepeticion,
                    intervalo,
                    unidadPersonalizada
                );

                if (fechaCalculada > fechaFinal)
                {
                    break;
                }

                fechas.Add(fechaCalculada);
                numeroRepeticion++;

                if (fechas.Count >= MaximoTurnosPorSerie)
                {
                    throw new ApplicationException(
                        $"Una serie no puede contener más de {MaximoTurnosPorSerie} turnos."
                    );
                }
            }

            return fechas;
        }

        private static DateTime CalcularFecha(
            DateTime fechaInicial,
            EnumFrecuenciaRecurrenciaTurno frecuencia,
            int numeroRepeticion,
            int intervalo,
            EnumUnidadRecurrenciaTurno? unidadPersonalizada
        )
        {
            return frecuencia switch
            {
                EnumFrecuenciaRecurrenciaTurno.semanal =>
                    fechaInicial.AddDays(
                        numeroRepeticion * 7
                    ),

                EnumFrecuenciaRecurrenciaTurno.cadaQuinceDias =>
                    fechaInicial.AddDays(
                        numeroRepeticion * 15
                    ),

                EnumFrecuenciaRecurrenciaTurno.mensual =>
                    fechaInicial.AddMonths(
                        numeroRepeticion
                    ),

                EnumFrecuenciaRecurrenciaTurno.personalizado =>
                    CalcularFechaPersonalizada(
                        fechaInicial,
                        numeroRepeticion,
                        intervalo,
                        unidadPersonalizada!.Value
                    ),

                _ => throw new ApplicationException(
                    "La frecuencia de repetición seleccionada no es válida."
                )
            };
        }

        private static DateTime CalcularFechaPersonalizada(
            DateTime fechaInicial,
            int numeroRepeticion,
            int intervalo,
            EnumUnidadRecurrenciaTurno unidad
        )
        {
            var cantidad =
                numeroRepeticion * intervalo;

            return unidad switch
            {
                EnumUnidadRecurrenciaTurno.dias =>
                    fechaInicial.AddDays(cantidad),

                EnumUnidadRecurrenciaTurno.semanas =>
                    fechaInicial.AddDays(
                        cantidad * 7
                    ),

                EnumUnidadRecurrenciaTurno.meses =>
                    fechaInicial.AddMonths(cantidad),

                _ => throw new ApplicationException(
                    "La unidad de repetición seleccionada no es válida."
                )
            };
        }
    }
}
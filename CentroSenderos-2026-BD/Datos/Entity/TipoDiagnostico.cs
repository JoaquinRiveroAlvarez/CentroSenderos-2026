using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoDiagnostico_Tipo_UQ", IsUnique = true)]
    public class TipoDiagnostico : EntityTipoBase
    {
        public List<Paciente> Pacientes { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    public class Socio : EntityBase
    {
        public int ProfesionalId { get; set; }
        public Profesional? Profesionales { get; set; }


        public List<GastoSocio> GastoSocios { get; set; } = new();

    }
}

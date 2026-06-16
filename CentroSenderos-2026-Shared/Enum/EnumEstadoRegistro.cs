using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentroSenderos_2026_Shared.Enum
{
    public enum EnumEstadoRegistro
    {
        activo = 1,
        inactivo = 2,
        borrado = 3,
        EnGrabacion = 4
    }
    public enum ResultadoOperacionSeguridad
    {
        Exitoso = 1,
        Fallido = 2,
        NoEncontrado = 3,
        SinPermiso = 4
    }
}

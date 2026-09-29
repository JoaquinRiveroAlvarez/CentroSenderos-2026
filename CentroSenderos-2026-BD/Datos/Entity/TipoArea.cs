using Microsoft.EntityFrameworkCore;

namespace CentroSenderos_2026_BD.Datos.Entity
{
    [Index(nameof(Tipo), Name = "TipoArea_Tipo_UQ", IsUnique = true)]
    public class TipoArea : EntityTipoBase
    {
    }
}
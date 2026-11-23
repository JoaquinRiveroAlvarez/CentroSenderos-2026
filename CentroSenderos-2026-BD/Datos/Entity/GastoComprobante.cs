using System.ComponentModel.DataAnnotations;

namespace CentroSenderos_2026_BD.Datos.Entity;

public sealed class GastoComprobante
{
    public int Id { get; set; }
    public int GastoId { get; set; }
    public Gasto Gasto { get; set; } = null!;

    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(40)]
    public string TipoContenido { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = [];
    public int TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
}
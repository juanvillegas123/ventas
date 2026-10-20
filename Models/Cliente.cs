using System.Text.Json.Serialization;

namespace sistema_ventas.Models;
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    // Cliente tiene muchas ventas
    [JsonIgnore]
    public List<Venta> Ventas { get; set; } = new();
}

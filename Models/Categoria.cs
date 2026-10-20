using System.Text.Json.Serialization;

namespace sistema_ventas.Models;
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    // relacion categoria y producto
    [JsonIgnore]
    public List<Producto> Productos { get; set; } = new();
}

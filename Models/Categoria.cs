using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace sistema_ventas.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    // Relación categoría y producto
    [JsonIgnore]
    public List<Producto> Productos { get; set; } = new();
}

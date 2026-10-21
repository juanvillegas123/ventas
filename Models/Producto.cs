using System.ComponentModel.DataAnnotations;

namespace sistema_ventas.Models;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock debe ser un entero no negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock mínimo debe ser un entero no negativo.")]
    public int Stock_minimo { get; set; }

    // Relación con categoría
    [Required(ErrorMessage = "Debe seleccionar una categoría válida.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    // Métodos
    public bool TieneStock()
    {
        return Stock > 0;
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace sistema_ventas.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser de al menos 1 unidad.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Required]
    [Range(0.01, 99999999.99, ErrorMessage = "El subtotal debe ser mayor a 0.")]
    public decimal Subtotal { get; set; }

    // Relaciones
    public int VentaId { get; set; }

    [Required(ErrorMessage = "Debe especificar un producto válido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe especificar un producto válido.")]
    public int ProductoId { get; set; }

    // Propiedades de navegación
    [JsonIgnore]
    public Venta? Venta { get; set; }
    public Producto? Producto { get; set; }

    // Métodos
    public void CalcularSubtotal()
    {
        Subtotal = Precio * Cantidad;
    }
}

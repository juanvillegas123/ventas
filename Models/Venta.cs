using System.ComponentModel.DataAnnotations;

namespace sistema_ventas.Models;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Debe asignar un cliente a la venta.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un cliente válido a la venta.")]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    [Range(0.0, 99999999.99, ErrorMessage = "El total no puede ser negativo.")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "El monto recibido es obligatorio.")]
    [Range(0.0, 99999999.99, ErrorMessage = "El monto recibido no puede ser negativo.")]
    public decimal Recibido { get; set; }

    [Range(0.0, 99999999.99, ErrorMessage = "El cambio no puede ser negativo.")]
    public decimal Cambio { get; set; }

    public List<DetalleVenta> DetalleVentas { get; set; } = new();

    public void CalcularTotal()
    { 
        Total = DetalleVentas.Sum(d => d.Subtotal);
    }

    public void CalcularCambio()
    { 
        Cambio = Recibido - Total;
    }

    public void ActualizarStock()
    { 
        foreach (var d in DetalleVentas)
        {
            if (d.Producto != null)
            {
                d.Producto.Stock -= d.Cantidad;
            }
        }
    }
}

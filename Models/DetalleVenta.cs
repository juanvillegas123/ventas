using System.Text.Json.Serialization;

namespace sistema_ventas.Models;
public class DetalleVenta
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal { get; set; }

    //Relaciones
    public int VentaId { get; set; }
    public int ProductoId { get; set; }

    //propiedades de navegacion
    [JsonIgnore]
    public Venta? Venta { get; set; }
    public Producto? Producto { get; set; }
    //metodos
    public void CalcularSubtotal()
    {
        Subtotal = Precio * Cantidad;
    }
}

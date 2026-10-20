namespace sistema_ventas.Models;
public class Venta{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public decimal Total { get; set; }
    public decimal Recibido { get; set; }
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
        foreach(var d in DetalleVentas)
            {
            if(d.Producto != null)
                {
                d.Producto.Stock -= d.Cantidad;
                }
            }
        }
}

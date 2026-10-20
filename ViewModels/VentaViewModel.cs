namespace sistema_ventas.ViewModels;

public class VentaItemDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}

public class ProcesarVentaDto
{
    public int ClienteId { get; set; }
    public decimal Recibido { get; set; }
    public List<VentaItemDto> Items { get; set; } = new();
}

public class NuevoClienteRapidoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

using sistema_ventas.Models;

namespace sistema_ventas.ViewModels;

public class ProductoTopDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalRecaudado { get; set; }
}

public class CategoriaVentaDto
{
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int CantidadProductosVendidos { get; set; }
    public decimal TotalRecaudado { get; set; }
    public double Porcentaje { get; set; }
}

public class VentaDiaDto
{
    public string Dia { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
}

public class DashboardViewModel
{
    // Ventas de hoy
    public decimal VentasHoy { get; set; }
    public int CantidadVentasHoy { get; set; }

    // Ventas del mes actual
    public decimal VentasMes { get; set; }
    public int CantidadVentasMes { get; set; }

    // Ventas del año actual
    public decimal VentasAnio { get; set; }
    public int CantidadVentasAnio { get; set; }

    // Total histórico de ventas
    public decimal TotalHistorico { get; set; }
    public int TotalVentasHistorico { get; set; }

    // Ticket promedio de compra
    public decimal TicketPromedio { get; set; }

    // Inventario y Directorio
    public int TotalClientes { get; set; }
    public int TotalProductos { get; set; }
    public int TotalCategorias { get; set; }

    // Alertas de inventario
    public List<Producto> ProductosStockBajo { get; set; } = new();

    // Actividad reciente
    public List<Venta> UltimasVentas { get; set; } = new();

    // Top productos más vendidos
    public List<ProductoTopDto> ProductosMasVendidos { get; set; } = new();

    // Ventas por Categoría
    public List<CategoriaVentaDto> VentasPorCategorias { get; set; } = new();

    // Tendencia de ventas en los últimos 7 días
    public List<VentaDiaDto> VentasUltimos7Dias { get; set; } = new();
}

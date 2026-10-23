using System.ComponentModel.DataAnnotations;
using sistema_ventas.Models;

namespace sistema_ventas.ViewModels;

public class ReportesIndexViewModel
{
    public string ActiveTab { get; set; } = "ventas"; // "ventas", "inventario", "clientes"
    public ReporteVentasViewModel VentasReporte { get; set; } = new();
    public ReporteInventarioViewModel InventarioReporte { get; set; } = new();
    public ReporteClientesViewModel ClientesReporte { get; set; } = new();
}

public class ReporteVentasViewModel
{
    // Filtros
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? ClienteId { get; set; }
    public string Preset { get; set; } = "mes"; // "hoy", "semana", "mes", "anio", "todos"

    // Opciones para combos
    public List<Cliente> ListaClientes { get; set; } = new();

    // Métricas Resumen (KPIs)
    public decimal TotalFacturado { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TicketPromedio { get; set; }
    public int TotalUnidadesVendidas { get; set; }

    // Registros
    public List<VentaReporteItem> Ventas { get; set; } = new();
}

public class VentaReporteItem
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Recibido { get; set; }
    public decimal Cambio { get; set; }
    public int ItemsCount { get; set; }
    public List<DetalleItemReporte> Detalles { get; set; } = new();
}

public class DetalleItemReporte
{
    public string ProductoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal { get; set; }
}

public class ReporteInventarioViewModel
{
    // Filtros
    public int? CategoriaId { get; set; }
    public string EstadoStock { get; set; } = "todos"; // "todos", "normal", "bajo", "agotado"
    public string? TerminoBusqueda { get; set; }

    // Opciones para combos
    public List<Categoria> ListaCategorias { get; set; } = new();

    // KPIs
    public decimal ValorizacionTotal { get; set; }
    public int TotalProductos { get; set; }
    public int TotalUnidadesEnStock { get; set; }
    public int ProductosStockBajo { get; set; }
    public int ProductosSinStock { get; set; }

    // Registros
    public List<ProductoInventarioItem> Productos { get; set; } = new();
}

public class ProductoInventarioItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public decimal ValorTotalStock { get; set; }
    public string Estado { get; set; } = "Normal"; // "Normal", "Bajo", "Agotado"
}

public class ReporteClientesViewModel
{
    // Filtros
    public string? TerminoBusqueda { get; set; }
    public string OrdenarPor { get; set; } = "mas_gastado"; // "mas_gastado", "mas_compras", "reciente", "nombre"

    // KPIs
    public int TotalClientes { get; set; }
    public int ClientesConCompras { get; set; }
    public decimal TotalHistoricoCompras { get; set; }
    public decimal PromedioPorCliente { get; set; }

    // Registros
    public List<ClienteActividadItem> Clientes { get; set; } = new();
}

public class ClienteActividadItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TotalComprado { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public decimal TicketPromedio { get; set; }
}

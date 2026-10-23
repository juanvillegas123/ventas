using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.ViewModels;

namespace sistema_ventas.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Reportes
    public async Task<IActionResult> Index(
        string tab = "ventas",
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        int? clienteId = null,
        string preset = "mes",
        int? categoriaId = null,
        string estadoStock = "todos",
        string? buscarProducto = null,
        string? buscarCliente = null,
        string ordenarClientes = "mas_gastado")
    {
        var model = new ReportesIndexViewModel
        {
            ActiveTab = string.IsNullOrEmpty(tab) ? "ventas" : tab.ToLower()
        };

        // ==========================================
        // 1. REPORTE DE VENTAS
        // ==========================================
        var ahora = DateTime.Now;
        var hoy = DateTime.Today;

        // Si se seleccionó preset y no se especificaron fechas manuales, calculamos el rango
        if (string.IsNullOrEmpty(Request.Query["fechaInicio"]) && string.IsNullOrEmpty(Request.Query["fechaFin"]))
        {
            switch (preset?.ToLower())
            {
                case "hoy":
                    fechaInicio = hoy;
                    fechaFin = hoy;
                    break;
                case "semana":
                    // Inicio de semana (Lunes)
                    int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fechaInicio = hoy.AddDays(-1 * diff);
                    fechaFin = hoy;
                    break;
                case "mes":
                    fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    fechaFin = hoy;
                    break;
                case "anio":
                    fechaInicio = new DateTime(hoy.Year, 1, 1);
                    fechaFin = hoy;
                    break;
                case "todos":
                    fechaInicio = null;
                    fechaFin = null;
                    break;
                default:
                    fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    fechaFin = hoy;
                    preset = "mes";
                    break;
            }
        }

        var queryVentas = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
            .AsQueryable();

        if (fechaInicio.HasValue)
        {
            var inicio = fechaInicio.Value.Date;
            queryVentas = queryVentas.Where(v => v.Fecha >= inicio);
        }

        if (fechaFin.HasValue)
        {
            var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
            queryVentas = queryVentas.Where(v => v.Fecha <= fin);
        }

        if (clienteId.HasValue && clienteId.Value > 0)
        {
            queryVentas = queryVentas.Where(v => v.ClienteId == clienteId.Value);
        }

        var ventasList = await queryVentas.OrderByDescending(v => v.Fecha).ToListAsync();

        var ventasReporteItems = ventasList.Select(v => new VentaReporteItem
        {
            Id = v.Id,
            Fecha = v.Fecha,
            ClienteNombre = v.Cliente?.Nombre ?? "Cliente General",
            Total = v.Total,
            Recibido = v.Recibido,
            Cambio = v.Cambio,
            ItemsCount = v.DetalleVentas.Sum(d => d.Cantidad),
            Detalles = v.DetalleVentas.Select(d => new DetalleItemReporte
            {
                ProductoNombre = d.Producto?.Nombre ?? "Producto no disponible",
                CategoriaNombre = d.Producto?.Categoria?.Nombre ?? "Sin categoría",
                Cantidad = d.Cantidad,
                Precio = d.Precio,
                Subtotal = d.Subtotal
            }).ToList()
        }).ToList();

        decimal totalFacturado = ventasReporteItems.Sum(v => v.Total);
        int totalVentas = ventasReporteItems.Count;
        int totalUnidades = ventasReporteItems.Sum(v => v.ItemsCount);
        decimal ticketPromedio = totalVentas > 0 ? (totalFacturado / totalVentas) : 0;

        model.VentasReporte = new ReporteVentasViewModel
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            ClienteId = clienteId,
            Preset = preset ?? "mes",
            ListaClientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync(),
            TotalFacturado = totalFacturado,
            CantidadVentas = totalVentas,
            TicketPromedio = ticketPromedio,
            TotalUnidadesVendidas = totalUnidades,
            Ventas = ventasReporteItems
        };

        // ==========================================
        // 2. REPORTE DE INVENTARIO Y STOCK
        // ==========================================
        var queryProductos = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            queryProductos = queryProductos.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscarProducto))
        {
            var busqueda = buscarProducto.Trim().ToLower();
            queryProductos = queryProductos.Where(p => p.Nombre.ToLower().Contains(busqueda));
        }

        var productosDb = await queryProductos.OrderBy(p => p.Categoria!.Nombre).ThenBy(p => p.Nombre).ToListAsync();

        var inventarioItems = productosDb.Select(p =>
        {
            string estado = "Normal";
            if (p.Stock == 0)
            {
                estado = "Agotado";
            }
            else if (p.Stock <= p.Stock_minimo)
            {
                estado = "Bajo";
            }

            return new ProductoInventarioItem
            {
                Id = p.Id,
                Nombre = p.Nombre,
                CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría",
                Precio = p.Precio,
                Stock = p.Stock,
                StockMinimo = p.Stock_minimo,
                ValorTotalStock = p.Stock * p.Precio,
                Estado = estado
            };
        }).ToList();

        // Aplicar filtro de estado de stock en memoria
        if (!string.IsNullOrEmpty(estadoStock) && estadoStock.ToLower() != "todos")
        {
            inventarioItems = inventarioItems.Where(i => i.Estado.ToLower() == estadoStock.ToLower()).ToList();
        }

        model.InventarioReporte = new ReporteInventarioViewModel
        {
            CategoriaId = categoriaId,
            EstadoStock = estadoStock ?? "todos",
            TerminoBusqueda = buscarProducto,
            ListaCategorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
            ValorizacionTotal = inventarioItems.Sum(i => i.ValorTotalStock),
            TotalProductos = inventarioItems.Count,
            TotalUnidadesEnStock = inventarioItems.Sum(i => i.Stock),
            ProductosStockBajo = inventarioItems.Count(i => i.Estado == "Bajo"),
            ProductosSinStock = inventarioItems.Count(i => i.Estado == "Agotado"),
            Productos = inventarioItems
        };

        // ==========================================
        // 3. REPORTE DE CLIENTES Y COMPORTAMIENTO
        // ==========================================
        var queryClientes = _context.Clientes
            .Include(c => c.Ventas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscarCliente))
        {
            var busqueda = buscarCliente.Trim().ToLower();
            queryClientes = queryClientes.Where(c => 
                c.Nombre.ToLower().Contains(busqueda) || 
                (c.Email != null && c.Email.ToLower().Contains(busqueda)) ||
                (c.Telefono != null && c.Telefono.ToLower().Contains(busqueda)));
        }

        var clientesDb = await queryClientes.ToListAsync();

        var clienteItems = clientesDb.Select(c =>
        {
            int cantVentas = c.Ventas.Count;
            decimal totalGasto = c.Ventas.Sum(v => v.Total);
            DateTime? ultCompra = c.Ventas.OrderByDescending(v => v.Fecha).Select(v => (DateTime?)v.Fecha).FirstOrDefault();
            decimal ticket = cantVentas > 0 ? (totalGasto / cantVentas) : 0;

            return new ClienteActividadItem
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                CantidadVentas = cantVentas,
                TotalComprado = totalGasto,
                UltimaCompra = ultCompra,
                TicketPromedio = ticket
            };
        }).ToList();

        // Ordenamiento
        clienteItems = (ordenarClientes?.ToLower()) switch
        {
            "mas_compras" => clienteItems.OrderByDescending(c => c.CantidadVentas).ThenByDescending(c => c.TotalComprado).ToList(),
            "reciente" => clienteItems.OrderByDescending(c => c.UltimaCompra ?? DateTime.MinValue).ToList(),
            "nombre" => clienteItems.OrderBy(c => c.Nombre).ToList(),
            _ => clienteItems.OrderByDescending(c => c.TotalComprado).ThenByDescending(c => c.CantidadVentas).ToList()
        };

        int totalClientesCount = clienteItems.Count;
        int clientesConComprasCount = clienteItems.Count(c => c.CantidadVentas > 0);
        decimal totalGastoGeneral = clienteItems.Sum(c => c.TotalComprado);
        decimal promedioPorCliente = clientesConComprasCount > 0 ? (totalGastoGeneral / clientesConComprasCount) : 0;

        model.ClientesReporte = new ReporteClientesViewModel
        {
            TerminoBusqueda = buscarCliente,
            OrdenarPor = ordenarClientes ?? "mas_gastado",
            TotalClientes = totalClientesCount,
            ClientesConCompras = clientesConComprasCount,
            TotalHistoricoCompras = totalGastoGeneral,
            PromedioPorCliente = promedioPorCliente,
            Clientes = clienteItems
        };

        return View(model);
    }

    // ==========================================
    // EXPORTACIÓN A EXCEL / CSV CON BOM UTF-8
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> ExportarVentasCsv(DateTime? fechaInicio, DateTime? fechaFin, int? clienteId)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(v => v.Fecha >= fechaInicio.Value.Date);

        if (fechaFin.HasValue)
            query = query.Where(v => v.Fecha <= fechaFin.Value.Date.AddDays(1).AddTicks(-1));

        if (clienteId.HasValue && clienteId.Value > 0)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

        var sb = new StringBuilder();
        // BOM UTF-8 para compatibilidad perfecta con Excel
        sb.Append('\uFEFF');

        // Encabezados
        sb.AppendLine("ID Venta;Fecha;Hora;Cliente;Total Articulos;Total Venta ($);Recibido ($);Cambio ($);Detalle de Productos");

        foreach (var v in ventas)
        {
            var detallesStr = string.Join(" | ", v.DetalleVentas.Select(d => $"{d.Cantidad}x {d.Producto?.Nombre} (${d.Precio:F2}) = ${d.Subtotal:F2}"));
            
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0};\"{1}\";\"{2}\";\"{3}\";{4};{5:F2};{6:F2};{7:F2};\"{8}\"",
                v.Id,
                v.Fecha.ToString("dd/MM/yyyy"),
                v.Fecha.ToString("HH:mm:ss"),
                v.Cliente?.Nombre ?? "Cliente General",
                v.DetalleVentas.Sum(d => d.Cantidad),
                v.Total,
                v.Recibido,
                v.Cambio,
                detallesStr.Replace("\"", "\"\"")));
        }

        var fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarInventarioCsv(int? categoriaId, string? estadoStock)
    {
        var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        var productos = await query.OrderBy(p => p.Categoria!.Nombre).ThenBy(p => p.Nombre).ToListAsync();

        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        sb.AppendLine("ID;Producto;Categoria;Precio Unitario ($);Stock Actual;Stock Minimo;Valorizacion Total ($);Estado de Stock");

        foreach (var p in productos)
        {
            string estado = "Normal";
            if (p.Stock == 0) estado = "Agotado";
            else if (p.Stock <= p.Stock_minimo) estado = "Bajo";

            if (!string.IsNullOrEmpty(estadoStock) && estadoStock.ToLower() != "todos" && estado.ToLower() != estadoStock.ToLower())
                continue;

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0};\"{1}\";\"{2}\";{3:F2};{4};{5};{6:F2};\"{7}\"",
                p.Id,
                p.Nombre.Replace("\"", "\"\""),
                p.Categoria?.Nombre ?? "Sin categoría",
                p.Precio,
                p.Stock,
                p.Stock_minimo,
                p.Stock * p.Precio,
                estado));
        }

        var fileName = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarClientesCsv(string? buscarCliente, string ordenarClientes = "mas_gastado")
    {
        var query = _context.Clientes.Include(c => c.Ventas).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscarCliente))
        {
            var b = buscarCliente.Trim().ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(b) || (c.Email != null && c.Email.ToLower().Contains(b)));
        }

        var clientes = await query.ToListAsync();

        var sb = new StringBuilder();
        sb.Append('\uFEFF');

        sb.AppendLine("ID;Nombre / Razon Social;Email;Telefono;Cantidad de Compras;Total Comprado ($);Ticket Promedio ($);Ultima Compra");

        foreach (var c in clientes.OrderByDescending(c => c.Ventas.Sum(v => v.Total)))
        {
            int cant = c.Ventas.Count;
            decimal total = c.Ventas.Sum(v => v.Total);
            decimal ticket = cant > 0 ? (total / cant) : 0;
            var ult = c.Ventas.OrderByDescending(v => v.Fecha).Select(v => (DateTime?)v.Fecha).FirstOrDefault();

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0};\"{1}\";\"{2}\";\"{3}\";{4};{5:F2};{6:F2};\"{7}\"",
                c.Id,
                c.Nombre.Replace("\"", "\"\""),
                c.Email ?? "N/A",
                c.Telefono ?? "N/A",
                cant,
                total,
                ticket,
                ult.HasValue ? ult.Value.ToString("dd/MM/yyyy HH:mm") : "Sin compras"));
        }

        var fileName = $"Reporte_Clientes_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }
}

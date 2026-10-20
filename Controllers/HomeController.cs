using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.ViewModels;

namespace sistema_ventas.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var inicioAnio = new DateTime(hoy.Year, 1, 1);

        // 1. Consultas de ventas por períodos
        var queryHoy = _context.Ventas.Where(v => v.Fecha >= hoy && v.Fecha < manana);
        var queryMes = _context.Ventas.Where(v => v.Fecha >= inicioMes);
        var queryAnio = _context.Ventas.Where(v => v.Fecha >= inicioAnio);

        var ventasHoy = await queryHoy.SumAsync(v => (decimal?)v.Total) ?? 0;
        var cantHoy = await queryHoy.CountAsync();

        var ventasMes = await queryMes.SumAsync(v => (decimal?)v.Total) ?? 0;
        var cantMes = await queryMes.CountAsync();

        var ventasAnio = await queryAnio.SumAsync(v => (decimal?)v.Total) ?? 0;
        var cantAnio = await queryAnio.CountAsync();

        var totalHistorico = await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0;
        var totalVentasHistorico = await _context.Ventas.CountAsync();

        var ticketPromedio = totalVentasHistorico > 0 ? totalHistorico / totalVentasHistorico : 0;

        // 2. Productos más vendidos
        var topProductos = await _context.DetalleVentas
            .Include(d => d.Producto)
                .ThenInclude(p => p!.Categoria)
            .Where(d => d.Producto != null)
            .GroupBy(d => new 
            { 
                d.ProductoId, 
                d.Producto!.Nombre, 
                CategoriaNombre = d.Producto.Categoria != null ? d.Producto.Categoria.Nombre : "General" 
            })
            .Select(g => new ProductoTopDto
            {
                ProductoId = g.Key.ProductoId,
                Nombre = g.Key.Nombre,
                CategoriaNombre = g.Key.CategoriaNombre,
                CantidadVendida = g.Sum(x => x.Cantidad),
                TotalRecaudado = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .Take(5)
            .ToListAsync();

        // 3. Ventas por Categoría
        var detallesConCategoria = await _context.DetalleVentas
            .Include(d => d.Producto)
                .ThenInclude(p => p!.Categoria)
            .Where(d => d.Producto != null && d.Producto.Categoria != null)
            .ToListAsync();

        var agrupadoCat = detallesConCategoria
            .GroupBy(d => new { d.Producto!.CategoriaId, d.Producto.Categoria!.Nombre })
            .Select(g => new
            {
                CategoriaId = g.Key.CategoriaId,
                Nombre = g.Key.Nombre,
                Cantidad = g.Sum(x => x.Cantidad),
                Total = g.Sum(x => x.Subtotal)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var granTotalCategorias = agrupadoCat.Sum(x => x.Total);

        var ventasPorCategorias = agrupadoCat.Select(c => new CategoriaVentaDto
        {
            CategoriaId = c.CategoriaId,
            CategoriaNombre = c.Nombre,
            CantidadProductosVendidos = c.Cantidad,
            TotalRecaudado = c.Total,
            Porcentaje = granTotalCategorias > 0 ? (double)Math.Round((c.Total / granTotalCategorias) * 100, 1) : 0
        }).ToList();

        // 4. Ventas en los últimos 7 días
        var hace7Dias = hoy.AddDays(-6);
        var ventas7Dias = await _context.Ventas
            .Where(v => v.Fecha >= hace7Dias)
            .ToListAsync();

        var ultimos7DiasList = new List<VentaDiaDto>();
        for (int i = 0; i < 7; i++)
        {
            var dia = hace7Dias.AddDays(i);
            var ventasDia = ventas7Dias.Where(v => v.Fecha.Date == dia.Date).ToList();
            ultimos7DiasList.Add(new VentaDiaDto
            {
                Dia = dia.ToString("ddd dd", new System.Globalization.CultureInfo("es-ES")),
                Total = ventasDia.Sum(v => v.Total),
                Cantidad = ventasDia.Count
            });
        }

        var viewModel = new DashboardViewModel
        {
            VentasHoy = ventasHoy,
            CantidadVentasHoy = cantHoy,
            VentasMes = ventasMes,
            CantidadVentasMes = cantMes,
            VentasAnio = ventasAnio,
            CantidadVentasAnio = cantAnio,
            TotalHistorico = totalHistorico,
            TotalVentasHistorico = totalVentasHistorico,
            TicketPromedio = ticketPromedio,

            TotalClientes = await _context.Clientes.CountAsync(),
            TotalProductos = await _context.Productos.CountAsync(),
            TotalCategorias = await _context.Categorias.CountAsync(),

            ProductosStockBajo = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Stock <= p.Stock_minimo)
                .OrderBy(p => p.Stock)
                .ToListAsync(),

            UltimasVentas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.DetalleVentas)
                .OrderByDescending(v => v.Fecha)
                .Take(6)
                .ToListAsync(),

            ProductosMasVendidos = topProductos,
            VentasPorCategorias = ventasPorCategorias,
            VentasUltimos7Dias = ultimos7DiasList
        };

        return View(viewModel);
    }

    public IActionResult Error()
    {
        return Content("Ocurrió un error. Intenta nuevamente.");
    }
}

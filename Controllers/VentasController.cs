using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;
using sistema_ventas.ViewModels;

namespace sistema_ventas.Controllers;

[Authorize]
public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Ventas
    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return View(ventas);
    }

    // GET: /Ventas/Create (Punto de Venta / POS)
    public async Task<IActionResult> Create()
    {
        // Asegurar que exista un Cliente General por defecto
        var clienteGeneral = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre == "Cliente General");
        if (clienteGeneral == null)
        {
            clienteGeneral = new Cliente { Nombre = "Cliente General" };
            _context.Clientes.Add(clienteGeneral);
            await _context.SaveChangesAsync();
        }

        ViewBag.ClienteGeneral = clienteGeneral;
        ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Productos = await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return View();
    }

    // POST: /Ventas/ProcesarVenta (Procesamiento transaccional de venta)
    [HttpPost]
    public async Task<IActionResult> ProcesarVenta([FromBody] ProcesarVentaDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { success = false, message = "Datos de venta inválidos." });
        }

        // Si no se especificó cliente o no existe, asignar automáticamente Cliente General
        if (dto.ClienteId <= 0 || !await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId))
        {
            var clienteGeneral = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre == "Cliente General");
            if (clienteGeneral == null)
            {
                clienteGeneral = new Cliente { Nombre = "Cliente General" };
                _context.Clientes.Add(clienteGeneral);
                await _context.SaveChangesAsync();
            }
            dto.ClienteId = clienteGeneral.Id;
        }

        if (dto.Items == null || dto.Items.Count == 0)
        {
            return BadRequest(new { success = false, message = "El carrito de ventas no puede estar vacío." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var venta = new Venta
            {
                ClienteId = dto.ClienteId,
                Fecha = DateTime.Now,
                Recibido = dto.Recibido
            };

            decimal totalCalculado = 0;

            foreach (var item in dto.Items)
            {
                if (item.Cantidad <= 0)
                {
                    return BadRequest(new { success = false, message = "La cantidad de cada producto debe ser mayor a 0." });
                }

                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null)
                {
                    return BadRequest(new { success = false, message = $"El producto seleccionado no existe (ID {item.ProductoId})." });
                }

                if (producto.Stock < item.Cantidad)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}." 
                    });
                }

                var subtotal = producto.Precio * item.Cantidad;
                totalCalculado += subtotal;

                // Descontar inventario
                producto.Stock -= item.Cantidad;

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    Precio = producto.Precio,
                    Subtotal = subtotal
                };

                venta.DetalleVentas.Add(detalle);
            }

            venta.Total = totalCalculado;

            if (dto.Recibido < totalCalculado)
            {
                return BadRequest(new { 
                    success = false, 
                    message = $"El monto recibido ({dto.Recibido:C}) no cubre el total de la venta ({totalCalculado:C})." 
                });
            }

            venta.Cambio = dto.Recibido - totalCalculado;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                ventaId = venta.Id,
                total = venta.Total,
                recibido = venta.Recibido,
                cambio = venta.Cambio,
                message = "Venta registrada exitosamente."
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { success = false, message = "Ocurrió un error al procesar la venta: " + ex.Message });
        }
    }

    // POST: /Ventas/CrearClienteRapido (Crear cliente inline desde el POS)
    [HttpPost]
    public async Task<IActionResult> CrearClienteRapido([FromBody] NuevoClienteRapidoDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return BadRequest(new { success = false, message = "El nombre del cliente es obligatorio." });
        }

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim()
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            id = cliente.Id,
            nombre = cliente.Nombre,
            email = cliente.Email ?? "",
            telefono = cliente.Telefono ?? ""
        });
    }

    // GET: /Ventas/Detalle/5 (Ticket de venta / Recibo)
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetalleVentas)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        return View(venta);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers;

public class ProductosController : Controller
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Productos
    public async Task<IActionResult> Index()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return View(productos);
    }

    // GET: /Productos/Create
    public async Task<IActionResult> Create()
    {
        await CargarCategoriasViewBag();
        return View(new Producto());
    }

    // POST: /Productos/Create
    [HttpPost]
    public async Task<IActionResult> Create(Producto producto)
    {
        // Validar que la categoría exista
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError("CategoriaId", "La categoría seleccionada no existe.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasViewBag();
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        await CargarCategoriasViewBag();
        return View(producto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();

        // Validar que la categoría exista
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError("CategoriaId", "La categoría seleccionada no existe.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasViewBag();
            return View(producto);
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto != null)
        {
            // No se puede eliminar un producto que tiene ventas asociadas
            var tieneVentas = await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id);
            if (tieneVentas)
            {
                ModelState.AddModelError("", "No se puede eliminar: el producto tiene ventas asociadas.");
                return View(producto);
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasViewBag()
    {
        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers;

[Authorize]
public class CategoriasController : Controller
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Categorias
    public async Task<IActionResult> Index()
    {
        return View(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync());
    }

    // GET: /Categorias/Create
    public IActionResult Create()
    {
        return View(new Categoria());
    }

    // POST: /Categorias/Create
    [HttpPost]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (!ModelState.IsValid) return View(categoria);

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categorias/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    // POST: /Categorias/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id) return NotFound();
        if (!ModelState.IsValid) return View(categoria);

        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categorias/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    // POST: /Categorias/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            // No se puede eliminar una categoría que tiene productos
            var tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (tieneProductos)
            {
                ModelState.AddModelError("", "No se puede eliminar: la categoría tiene productos.");
                return View(categoria);
            }
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

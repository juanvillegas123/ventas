using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers;

public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Clientes
    public async Task<IActionResult> Index()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Ventas)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return View(clientes);
    }

    // GET: /Clientes/Create
    public IActionResult Create()
    {
        return View(new Cliente());
    }

    // POST: /Clientes/Create
    [HttpPost]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();
        if (!ModelState.IsValid) return View(cliente);

        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            var tieneVentas = await _context.Ventas.AnyAsync(v => v.ClienteId == id);
            if (tieneVentas)
            {
                ModelState.AddModelError("", "No se puede eliminar: el cliente tiene ventas asociadas.");
                return View(cliente);
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

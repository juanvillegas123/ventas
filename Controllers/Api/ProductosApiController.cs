using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers.Api;

//este apicontroller valida un Json automaticamente 400,
//en caso de funcionar dara status 200

[ApiController]
[Route("api/productos")]
public class ProductosApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProductosApiController(AppDbContext context)
    {
        _context = context;
    }

    //GET api/productos
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> GetProductos()
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    //GET api/productos/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            return NotFound();
        }
        return producto;
    }

    //POST api/productos
    //Body:{"nombre":"Detergente","precio":12.5,"stock":20,"stock_minimo":5,"categoriaId":1}
    [HttpPost]
    public async Task<ActionResult<Producto>> Create(Producto producto)
    {
        //Validar que la categoría exista
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            return BadRequest("Error: La categoría especificada no existe");
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        // Cargar la relación Categoria para que no devuelva null en la respuesta
        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    //PUT api/productos/1
    //body:{"id":1,"nombre":"Jabon Líquido","precio":10,"stock":15,"stock_minimo":3,"categoriaId":1}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Producto producto)
    {
        if (id != producto.Id)
            return BadRequest("Error: No coincide el id");

        if (!await _context.Productos.AnyAsync(p => p.Id == id))
            return NotFound();

        //Validar que la categoría exista
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            return BadRequest("Error: La categoría especificada no existe");
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    //DELETE api/productos/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        //Si tiene detalles de venta relacionados, no permitir borrar
        if (await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id))
        {
            return BadRequest("No se puede borrar: tiene ventas asociadas");
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

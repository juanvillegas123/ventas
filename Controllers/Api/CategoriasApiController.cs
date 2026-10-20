using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers.Api;

//este apicontroller valida un Json automaticamente 400,
//en caso de funcionar dara status 200

[ApiController]
[Route("api/categorias")]
public class CategoriasApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public CategoriasApiController(AppDbContext context)
    {
        _context = context;
    }

    //GET api/categorias
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> GetCategorias()
    {
        return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
    }

    //GET api/categorias/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return categoria;
    }

    //POST api/categorias
    //Body:{"nombre":"limpieza"}
    [HttpPost]
    public async Task<ActionResult<Categoria>> Create(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    //PUT api/categorias/3
    //body:{"id:3","nombre:Bebidas Gaseosas"}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Categoria categoria)
    {
        if (id != categoria.Id)
            return BadRequest("Error: No coincide el id");

        if (!await _context.Categorias.AnyAsync(c => c.Id == id)) 
            return NotFound();
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    //DELETE api/categorias/4
    [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();
        //Si tiene productos relacionados, no permitir borrar
        if(await _context.Productos.AnyAsync(p=>p.CategoriaId==id))
        {
            return BadRequest("No se puede borrar: tiene productos relacionados");
        }
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
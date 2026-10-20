using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers.Api;

//este apicontroller valida un Json automaticamente 400,
//en caso de funcionar dara status 200

[ApiController]
[Route("api/detalleventas")]
public class DetalleVentasApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public DetalleVentasApiController(AppDbContext context)
    {
        _context = context;
    }

    //GET api/detalleventas
    [HttpGet]
    public async Task<ActionResult<List<DetalleVenta>>> GetDetalleVentas()
    {
        return await _context.DetalleVentas.ToListAsync();
    }

    //GET api/detalleventas/1
    [HttpGet("{id}")]
    public async Task<ActionResult<DetalleVenta>> GetById(int id)
    {
        var detalle = await _context.DetalleVentas.FindAsync(id);
        if (detalle == null)
        {
            return NotFound();
        }
        return detalle;
    }

    //POST api/detalleventas
    //Body:{"ventaId":1,"productoId":1,"cantidad":2,"precio":8,"subtotal":16}
    [HttpPost]
    public async Task<ActionResult<DetalleVenta>> Create(DetalleVenta detalle)
    {
        //Validar que la venta exista
        if (!await _context.Ventas.AnyAsync(v => v.Id == detalle.VentaId))
        {
            return BadRequest("Error: La venta especificada no existe");
        }

        //Validar que el producto exista
        if (!await _context.Productos.AnyAsync(p => p.Id == detalle.ProductoId))
        {
            return BadRequest("Error: El producto especificado no existe");
        }

        if (detalle.Subtotal == 0)
        {
            detalle.CalcularSubtotal();
        }

        _context.DetalleVentas.Add(detalle);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = detalle.Id }, detalle);
    }

    //PUT api/detalleventas/1
    //body:{"id":1,"ventaId":1,"productoId":1,"cantidad":3,"precio":8,"subtotal":24}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, DetalleVenta detalle)
    {
        if (id != detalle.Id)
            return BadRequest("Error: No coincide el id");

        if (!await _context.DetalleVentas.AnyAsync(d => d.Id == id))
            return NotFound();

        //Validar que la venta exista
        if (!await _context.Ventas.AnyAsync(v => v.Id == detalle.VentaId))
        {
            return BadRequest("Error: La venta especificada no existe");
        }

        //Validar que el producto exista
        if (!await _context.Productos.AnyAsync(p => p.Id == detalle.ProductoId))
        {
            return BadRequest("Error: El producto especificado no existe");
        }

        if (detalle.Subtotal == 0)
        {
            detalle.CalcularSubtotal();
        }

        _context.DetalleVentas.Update(detalle);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    //DELETE api/detalleventas/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var detalle = await _context.DetalleVentas.FindAsync(id);
        if (detalle == null) return NotFound();

        _context.DetalleVentas.Remove(detalle);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

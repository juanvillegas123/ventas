using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;

namespace sistema_ventas.Controllers.Api;

//este apicontroller valida un Json automaticamente 400,
//en caso de funcionar dara status 200

[ApiController]
[Route("api/ventas")]
public class VentasApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public VentasApiController(AppDbContext context)
    {
        _context = context;
    }

    //GET api/ventas
    [HttpGet]
    public async Task<ActionResult<List<Venta>>> GetVentas()
    {
        return await _context.Ventas.OrderByDescending(v => v.Fecha).ToListAsync();
    }

    //GET api/ventas/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Venta>> GetById(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta == null)
        {
            return NotFound();
        }
        return venta;
    }

    //POST api/ventas
    //Body:{"clienteId":1,"total":50,"recibido":100,"cambio":50}
    [HttpPost]
    public async Task<ActionResult<Venta>> Create(Venta venta)
    {
        //Validar que el cliente exista
        if (!await _context.Clientes.AnyAsync(c => c.Id == venta.ClienteId))
        {
            return BadRequest("Error: El cliente especificado no existe");
        }

        //Calcular totales si aplica
        if (venta.Total == 0 && venta.DetalleVentas.Count > 0)
        {
            venta.CalcularTotal();
        }
        venta.CalcularCambio();

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
    }

    //PUT api/ventas/1
    //body:{"id":1,"clienteId":1,"total":60,"recibido":100,"cambio":40}
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Venta venta)
    {
        if (id != venta.Id)
            return BadRequest("Error: No coincide el id");

        if (!await _context.Ventas.AnyAsync(v => v.Id == id))
            return NotFound();

        //Validar que el cliente exista
        if (!await _context.Clientes.AnyAsync(c => c.Id == venta.ClienteId))
        {
            return BadRequest("Error: El cliente especificado no existe");
        }

        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    //DELETE api/ventas/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta == null) return NotFound();

        //Si tiene detalles de venta relacionados, no permitir borrar
        if (await _context.DetalleVentas.AnyAsync(d => d.VentaId == id))
        {
            return BadRequest("No se puede borrar: tiene detalles de venta asociados");
        }

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

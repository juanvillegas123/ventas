using Microsoft.EntityFrameworkCore; //enlace bd modelos
using sistema_ventas.Models;
namespace sistema_ventas.Data;

public class AppDbContext : DbContext
{
    //Constructor que va a recibir las opciones de configuracion
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    //Defini los DbSet, que se traduciran en tablas de la bd
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

    //Metodos para configurar las restricciones, reaciones y datos iniciales
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //Insercion de datos iniciales a la tabla categorias
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Limpieza" },
            new Categoria { Id = 2, Nombre = "Abarrotes" },
            new Categoria { Id = 3, Nombre = "Bebidas" }
        );
        //insercion de datos iniciales a la tabla productos
        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Jabon", Precio = 8, Stock = 6, Stock_minimo = 2, CategoriaId = 1 },
            new Producto { Id = 2, Nombre = "Arroz", Precio = 16, Stock = 55, Stock_minimo = 10, CategoriaId = 2 },
            new Producto { Id = 3, Nombre = "Coca-Cola", Precio = 21, Stock = 150, Stock_minimo = 15, CategoriaId = 3 }
        );
    }
}

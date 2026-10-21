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

        // --- 1. CATEGORIA ---
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(c => c.Nombre)
                .IsUnique()
                .HasDatabaseName("IX_Categorias_Nombre_Unique");
        });

        // --- 2. PRODUCTO ---
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Precio)
                .HasPrecision(18, 2);

            entity.HasIndex(p => new { p.Nombre, p.CategoriaId })
                .IsUnique()
                .HasDatabaseName("IX_Productos_Nombre_CategoriaId_Unique");

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Productos_Precio", "Precio > 0");
                t.HasCheckConstraint("CK_Productos_Stock", "Stock >= 0");
                t.HasCheckConstraint("CK_Productos_Stock_minimo", "Stock_minimo >= 0");
            });

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 3. CLIENTE ---
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(c => c.Email)
                .HasMaxLength(150);

            entity.Property(c => c.Telefono)
                .HasMaxLength(20);

            entity.HasIndex(c => c.Email)
                .IsUnique()
                .HasFilter("Email IS NOT NULL")
                .HasDatabaseName("IX_Clientes_Email_Unique");
        });

        // --- 4. VENTA ---
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(v => v.Total)
                .HasPrecision(18, 2);

            entity.Property(v => v.Recibido)
                .HasPrecision(18, 2);

            entity.Property(v => v.Cambio)
                .HasPrecision(18, 2);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Ventas_Total", "Total >= 0");
                t.HasCheckConstraint("CK_Ventas_Recibido", "Recibido >= Total");
                t.HasCheckConstraint("CK_Ventas_Cambio", "Cambio >= 0");
            });

            entity.HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 5. DETALLE VENTA ---
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.Property(d => d.Precio)
                .HasPrecision(18, 2);

            entity.Property(d => d.Subtotal)
                .HasPrecision(18, 2);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_DetalleVentas_Cantidad", "Cantidad > 0");
                t.HasCheckConstraint("CK_DetalleVentas_Precio", "Precio > 0");
                t.HasCheckConstraint("CK_DetalleVentas_Subtotal", "Subtotal > 0");
            });

            entity.HasOne(d => d.Venta)
                .WithMany(v => v.DetalleVentas)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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

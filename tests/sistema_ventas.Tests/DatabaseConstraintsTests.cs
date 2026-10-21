using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using sistema_ventas.Data;
using sistema_ventas.Models;
using Xunit;

namespace sistema_ventas.Tests;

public class DatabaseConstraintsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public DatabaseConstraintsTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public void VAL_CAT_02_NombreCategoria_Duplicado_DebeLanzarExcepcion()
    {
        using var context = new AppDbContext(_options);

        context.Categorias.Add(new Categoria { Nombre = "Golosinas" });
        context.SaveChanges();

        context.Categorias.Add(new Categoria { Nombre = "Golosinas" });
        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*UNIQUE*");
    }

    [Fact]
    public void VAL_PROD_02_NombreProducto_DuplicadoEnMismaCategoria_DebeLanzarExcepcion()
    {
        using var context = new AppDbContext(_options);

        var cat = new Categoria { Nombre = "Lácteos" };
        context.Categorias.Add(cat);
        context.SaveChanges();

        context.Productos.Add(new Producto { Nombre = "Leche Entera", Precio = 10, Stock = 20, Stock_minimo = 5, CategoriaId = cat.Id });
        context.SaveChanges();

        context.Productos.Add(new Producto { Nombre = "Leche Entera", Precio = 12, Stock = 10, Stock_minimo = 2, CategoriaId = cat.Id });
        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*UNIQUE*");
    }

    [Fact]
    public void VAL_PROD_03_PrecioNegativoO_Cero_DebeViolarConstraintCHECK()
    {
        using var context = new AppDbContext(_options);

        var prod = new Producto { Nombre = "Producto Inválido", Precio = -5.00m, Stock = 10, Stock_minimo = 2, CategoriaId = 1 };
        context.Productos.Add(prod);

        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*CHECK*");
    }

    [Fact]
    public void VAL_PROD_04_StockNegativo_DebeViolarConstraintCHECK()
    {
        using var context = new AppDbContext(_options);

        var prod = new Producto { Nombre = "Sin Stock Inválido", Precio = 15.00m, Stock = -1, Stock_minimo = 2, CategoriaId = 1 };
        context.Productos.Add(prod);

        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*CHECK*");
    }

    [Fact]
    public void VAL_CLI_02_EmailDuplicado_DebeLanzarExcepcion()
    {
        using var context = new AppDbContext(_options);

        context.Clientes.Add(new Cliente { Nombre = "Juan Perez", Email = "juan@empresa.com" });
        context.SaveChanges();

        context.Clientes.Add(new Cliente { Nombre = "Juan Dos", Email = "juan@empresa.com" });
        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*UNIQUE*");
    }

    [Fact]
    public void VAL_VEN_05_RecibidoMenorQueTotal_DebeViolarConstraintCHECK()
    {
        using var context = new AppDbContext(_options);

        var venta = new Venta
        {
            ClienteId = 1,
            Total = 100.00m,
            Recibido = 50.00m,
            Cambio = -50.00m
        };
        context.Ventas.Add(venta);

        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*CHECK*");
    }

    [Fact]
    public void VAL_DET_01_CantidadCeroONegativa_DebeViolarConstraintCHECK()
    {
        using var context = new AppDbContext(_options);

        var detalle = new DetalleVenta
        {
            VentaId = 1,
            ProductoId = 1,
            Cantidad = 0,
            Precio = 10.00m,
            Subtotal = 0.00m
        };
        context.DetalleVentas.Add(detalle);

        Action act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<SqliteException>()
            .WithMessage("*CHECK*");
    }
}

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using sistema_ventas.Models;
using Xunit;

namespace sistema_ventas.Tests;

public class ModelValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")] // Menor a 3 caracteres
    public void VAL_CAT_01_NombreCategoriaInvalido_FallaValidacion(string nombreInvalido)
    {
        var cat = new Categoria { Nombre = nombreInvalido };
        var results = ValidateModel(cat);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Nombre"));
    }

    [Fact]
    public void VAL_CAT_01_NombreCategoriaValido_PasaValidacion()
    {
        var cat = new Categoria { Nombre = "Bebidas Alcohólicas" };
        var results = ValidateModel(cat);

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-10.0)]
    [InlineData(0.0)]
    public void VAL_PROD_03_PrecioProductoInvalido_FallaValidacion(decimal precioInvalido)
    {
        var prod = new Producto
        {
            Nombre = "Producto Test",
            Precio = precioInvalido,
            Stock = 10,
            Stock_minimo = 2,
            CategoriaId = 1
        };

        var results = ValidateModel(prod);
        results.Should().Contain(r => r.MemberNames.Contains("Precio"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-50)]
    public void VAL_PROD_04_StockNegativo_FallaValidacion(int stockInvalido)
    {
        var prod = new Producto
        {
            Nombre = "Producto Test",
            Precio = 15.0m,
            Stock = stockInvalido,
            Stock_minimo = 2,
            CategoriaId = 1
        };

        var results = ValidateModel(prod);
        results.Should().Contain(r => r.MemberNames.Contains("Stock"));
    }

    [Theory]
    [InlineData("correo-invalido")]
    [InlineData("sin_arroba.com")]
    [InlineData("@dominio.com")]
    public void VAL_CLI_02_EmailFormatoInvalido_FallaValidacion(string emailInvalido)
    {
        var cliente = new Cliente
        {
            Nombre = "Cliente Prueba",
            Email = emailInvalido
        };

        var results = ValidateModel(cliente);
        results.Should().Contain(r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void VAL_CLI_02_EmailFormatoValido_PasaValidacion()
    {
        var cliente = new Cliente
        {
            Nombre = "Cliente Prueba",
            Email = "contacto@empresa.com",
            Telefono = "+54 9 11 1234-5678"
        };

        var results = ValidateModel(cliente);
        results.Should().BeEmpty();
    }

    [Fact]
    public void VAL_VEN_04_CalculoTotal_DebeSumarSubtotalesCorrectamente()
    {
        var venta = new Venta
        {
            ClienteId = 1,
            Recibido = 100m,
            DetalleVentas = new List<DetalleVenta>
            {
                new DetalleVenta { ProductoId = 1, Cantidad = 2, Precio = 15m, Subtotal = 30m },
                new DetalleVenta { ProductoId = 2, Cantidad = 1, Precio = 25m, Subtotal = 25m }
            }
        };

        venta.CalcularTotal();
        venta.CalcularCambio();

        venta.Total.Should().Be(55m);
        venta.Cambio.Should().Be(45m);
    }

    [Fact]
    public void VAL_DET_02_CalcularSubtotal_DebeMultiplicarPrecioPorCantidad()
    {
        var detalle = new DetalleVenta
        {
            ProductoId = 1,
            Cantidad = 4,
            Precio = 12.50m
        };

        detalle.CalcularSubtotal();
        detalle.Subtotal.Should().Be(50.00m);
    }
}

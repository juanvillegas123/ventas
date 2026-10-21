using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sistema_ventas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRestriccionesValidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVentas_Productos_ProductoId",
                table: "DetalleVentas");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Cambio",
                table: "Ventas",
                sql: "Cambio >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Recibido",
                table: "Ventas",
                sql: "Recibido >= Total");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Total",
                table: "Ventas",
                sql: "Total >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre_CategoriaId_Unique",
                table: "Productos",
                columns: new[] { "Nombre", "CategoriaId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos",
                sql: "Precio > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos",
                sql: "Stock >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock_minimo",
                table: "Productos",
                sql: "Stock_minimo >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleVentas_Cantidad",
                table: "DetalleVentas",
                sql: "Cantidad > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleVentas_Precio",
                table: "DetalleVentas",
                sql: "Precio > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleVentas_Subtotal",
                table: "DetalleVentas",
                sql: "Subtotal > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email_Unique",
                table: "Clientes",
                column: "Email",
                unique: true,
                filter: "Email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre_Unique",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVentas_Productos_ProductoId",
                table: "DetalleVentas",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVentas_Productos_ProductoId",
                table: "DetalleVentas");

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Cambio",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Recibido",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Total",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Nombre_CategoriaId_Unique",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock_minimo",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetalleVentas_Cantidad",
                table: "DetalleVentas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetalleVentas_Precio",
                table: "DetalleVentas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetalleVentas_Subtotal",
                table: "DetalleVentas");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email_Unique",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nombre_Unique",
                table: "Categorias");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVentas_Productos_ProductoId",
                table: "DetalleVentas",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Categorias_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

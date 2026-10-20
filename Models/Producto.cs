namespace sistema_ventas.Models;using System.Data.Common;
public class Producto{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int Stock_minimo { get; set; }

    //relacion con categoria
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    //Metodos
    public bool TieneStock(){
        return Stock > 0;
    }
}

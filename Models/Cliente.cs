using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace sistema_ventas.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre o razón social del cliente es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string? Email { get; set; }

    [RegularExpression(@"^\+?[0-9\s-]{7,20}$", ErrorMessage = "El formato del teléfono no es válido (solo números, guiones y espacios, 7 a 20 dígitos).")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }

    // Cliente tiene muchas ventas
    [JsonIgnore]
    public List<Venta> Ventas { get; set; } = new();
}

using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using sistema_ventas.Data;

// 1) Cargar el archivo .env (ASP.NET Core no lo lee solo). En Railway no existe: se usan variables del panel.
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var linea in File.ReadAllLines(envFile))
    {
        var l = linea.Trim();
        if (l.Length == 0 || l.StartsWith('#') || !l.Contains('=')) continue;
        var partes = l.Split('=', 2);
        Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim().Trim('"'));
    }
}

var builder = WebApplication.CreateBuilder(args);

// IgnoreCycles evita el error de JSON circular: Producto -> Categoria -> Productos -> ...
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 2) Base de datos. El proveedor se cambia SOLO en esta línea.
//    Ahora: SQLite (archivo local). Ver README, sección "Conexión a Turso".
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") 
    ?? Path.Combine(builder.Environment.ContentRootPath, "ventas.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 3) Railway entrega el puerto en la variable PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_ventas.ViewModels;

namespace sistema_ventas.Controllers;

public class AccountController : Controller
{
    // Variables de acceso simples (no requieren conexión a base de datos)
    // Se pueden personalizar mediante variables de entorno si se desea
    private readonly string _usuarioValido;
    private readonly string _passwordValido;

    public AccountController()
    {
        _usuarioValido = Environment.GetEnvironmentVariable("AUTH_USER") ?? "admin";
        _passwordValido = Environment.GetEnvironmentVariable("AUTH_PASS") ?? "admin";
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si el usuario ya está autenticado, lo redirigimos directo al sistema
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(returnUrl ?? "/");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validación directa contra las variables definidas en memoria
        if (model.Usuario.Trim().Equals(_usuarioValido, StringComparison.OrdinalIgnoreCase) && 
            model.Password == _passwordValido)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Usuario.Trim()),
                new Claim(ClaimTypes.Role, "Administrador")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos. Verifique sus credenciales.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}

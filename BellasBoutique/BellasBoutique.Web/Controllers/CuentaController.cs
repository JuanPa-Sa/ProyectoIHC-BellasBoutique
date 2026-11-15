/* Responsable: Juan Pablo
   Login, Registro y Recuperar contraseña (módulos 1 y 2). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class CuentaController : Controller
{
    public IActionResult Login() => View();

    public IActionResult Registro() => View();

    public IActionResult RecuperarContrasena() => View();
}

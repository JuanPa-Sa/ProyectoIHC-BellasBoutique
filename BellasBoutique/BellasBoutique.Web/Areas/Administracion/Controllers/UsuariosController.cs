/* Responsable: Juan Pablo
   Administración de usuarios: estado de las cuentas y asignación de roles (módulo 7). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class UsuariosController : Controller
{
    public IActionResult Index() => View();
}

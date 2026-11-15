/* Responsable: Joel
   Página de inicio de la vista Administración. */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class HomeController : Controller
{
    public IActionResult Index() => View();
}

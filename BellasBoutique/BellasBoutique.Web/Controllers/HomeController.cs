/* Responsable: Joel
   Página de inicio y página de error de la vista Cliente. */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}

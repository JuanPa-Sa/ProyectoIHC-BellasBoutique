/* Responsable: Eduardo
   Chat en línea del vendedor (módulo 6). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class SoporteController : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(Chat));

    public IActionResult Chat() => View();
}

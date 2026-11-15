/* Responsable: Joel
   Historial de ventas (módulo 5). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class VentasController : Controller
{
    public IActionResult Index() => View();
}

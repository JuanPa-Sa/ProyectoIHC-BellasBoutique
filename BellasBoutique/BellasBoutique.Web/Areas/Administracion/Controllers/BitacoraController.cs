/* Responsable: Juan Pablo
   Bitácora de actividades: altas, bajas, modificaciones y compras (req. 8c). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class BitacoraController : Controller
{
    public IActionResult Index() => View();
}

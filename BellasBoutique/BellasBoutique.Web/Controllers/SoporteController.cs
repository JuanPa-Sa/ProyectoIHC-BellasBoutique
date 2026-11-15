/* Responsable: Eduardo
   Preguntas frecuentes, encuesta, sugerencias y chat del cliente (módulo 6). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class SoporteController : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(PreguntasFrecuentes));

    public IActionResult PreguntasFrecuentes() => View();

    public IActionResult Encuesta() => View();

    public IActionResult Sugerencias() => View();

    public IActionResult Chat() => View();
}

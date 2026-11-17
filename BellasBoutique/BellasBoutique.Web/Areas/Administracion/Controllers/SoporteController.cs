using Microsoft.AspNetCore.Mvc;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")]
public class SoporteController:Controller
{
    public IActionResult Index()=>RedirectToAction(nameof(Chat));
    public IActionResult Chat()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Chat(string mensaje){ViewBag.Pregunta=mensaje;ViewBag.Respuesta="Aqui iria el bot funcional";return View();}
}

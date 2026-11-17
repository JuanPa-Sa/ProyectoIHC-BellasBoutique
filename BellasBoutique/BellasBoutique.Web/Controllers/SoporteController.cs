using Microsoft.AspNetCore.Mvc;
namespace BellasBoutique.Web.Controllers;
public class SoporteController:Controller
{
    public IActionResult Index()=>RedirectToAction(nameof(PreguntasFrecuentes));
    public IActionResult PreguntasFrecuentes()=>View();
    public IActionResult Encuesta()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Encuesta(int puntaje,string? comentario){ViewBag.Aviso="Encuesta enviada (simulación). ¡Gracias!";return View();}
    public IActionResult Sugerencias()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Sugerencias(string sugerencia){ViewBag.Aviso="Sugerencia recibida (simulación).";return View();}
    public IActionResult Chat()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Chat(string mensaje){ViewBag.Pregunta=mensaje;ViewBag.Respuesta="Aqui iria el bot funcional";return View();}
}

using Microsoft.AspNetCore.Mvc;using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class SoporteController(TiendaDb db):Controller {
 private int U=>HttpContext.Session.GetInt32("UsuarioId")??0;
 public IActionResult Index()=>RedirectToAction(nameof(PreguntasFrecuentes));public IActionResult PreguntasFrecuentes()=>View();
 [HttpGet] public IActionResult Encuesta()=>View();[HttpPost,ValidateAntiForgeryToken] public IActionResult Encuesta(int puntaje,string? comentario){if(puntaje<1||puntaje>5||(comentario??"").Length>1000){ViewBag.Aviso="Revisá el puntaje y comentario.";return View();}db.Ejecutar("INSERT Encuestas(UsuarioId,Puntaje,Comentario) VALUES(@u,@p,@c)",("@u",U),("@p",puntaje),("@c",comentario??""));ViewBag.Aviso="Encuesta enviada. ¡Gracias!";return View();}
 [HttpGet] public IActionResult Sugerencias()=>View();[HttpPost,ValidateAntiForgeryToken] public IActionResult Sugerencias(string sugerencia){if(string.IsNullOrWhiteSpace(sugerencia)||sugerencia.Length>1000){ViewBag.Aviso="Escribí una sugerencia de hasta 1000 caracteres.";return View();}db.Ejecutar("INSERT Sugerencias(UsuarioId,Texto) VALUES(@u,@t)",("@u",U),("@t",sugerencia));ViewBag.Aviso="Sugerencia recibida.";return View();}
 [HttpGet] public IActionResult Chat()=>HttpContext.Session.GetString("Rol")=="Cliente"?View():Redirect("/Administracion/Soporte/Chat");
 [HttpGet] public IActionResult Mensajes()=>HttpContext.Session.GetString("Rol")=="Cliente"?Json(db.Chat(U)):StatusCode(403);
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Chat(string mensaje){if(HttpContext.Session.GetString("Rol")=="Cliente"&&!string.IsNullOrWhiteSpace(mensaje)&&mensaje.Length<=1000)db.Mensaje(U,U,mensaje);return RedirectToAction(nameof(Chat));}
}
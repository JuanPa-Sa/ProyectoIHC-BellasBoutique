using Microsoft.AspNetCore.Mvc;using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")] public class SoporteController(TiendaDb db):Controller{
 public IActionResult Index()=>RedirectToAction(nameof(Chat));
 [HttpGet] public IActionResult Chat(int? id){var lista=db.Conversaciones();ViewBag.Clientes=lista;ViewBag.Cliente=lista.FirstOrDefault(u=>u.Id==id)??lista.FirstOrDefault();return View();}
 [HttpGet] public IActionResult Mensajes(int id)=>Json(db.Chat(id));
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Chat(int id,string mensaje){if(db.UsuarioPorId(id)?.Rol=="Cliente"&&!string.IsNullOrWhiteSpace(mensaje)&&mensaje.Length<=1000)db.Mensaje(id,HttpContext.Session.GetInt32("UsuarioId")!.Value,mensaje);return RedirectToAction(nameof(Chat),new{id});}
 public IActionResult Feedback()=>View(db.Feedback());
}
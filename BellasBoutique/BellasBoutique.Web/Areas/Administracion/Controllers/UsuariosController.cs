using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")]
public class UsuariosController(TiendaDb db):Controller
{
    public IActionResult Index(){if(HttpContext.Session.GetString("Rol")!="Administrador")return Redirect("/Cuenta/Login");return View(db.Usuarios());}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Cambiar(int id,string rol,bool activo){if(HttpContext.Session.GetString("Rol")!="Administrador")return Redirect("/Cuenta/Login");if(new[]{"Cliente","Vendedor","Administrador"}.Contains(rol)&&id!=HttpContext.Session.GetInt32("UsuarioId"))db.ModificarUsuario(id,rol,activo);return RedirectToAction(nameof(Index));}
}

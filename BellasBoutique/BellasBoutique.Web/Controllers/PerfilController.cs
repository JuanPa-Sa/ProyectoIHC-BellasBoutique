using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class PerfilController(TiendaDb db):Controller
{
    public IActionResult Index(){var id=HttpContext.Session.GetInt32("UsuarioId");if(!id.HasValue)return RedirectToAction("Login","Cuenta");var u=db.UsuarioPorId(id.Value);return u==null?RedirectToAction("Login","Cuenta"):View(u);}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Index(string nombre,string apellidos,string telefono,string direccion){var id=HttpContext.Session.GetInt32("UsuarioId");if(!id.HasValue)return RedirectToAction("Login","Cuenta");db.EditarPerfil(id.Value,nombre??"",apellidos??"",telefono??"",direccion??"");TempData["Aviso"]="Perfil actualizado.";return RedirectToAction(nameof(Index));}
}

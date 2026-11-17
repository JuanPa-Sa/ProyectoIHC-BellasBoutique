using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class PerfilController(TiendaDb db):Controller
{
    public IActionResult Index(){var id=HttpContext.Session.GetInt32("UsuarioId");if(!id.HasValue)return RedirectToAction("Login","Cuenta");var u=db.UsuarioPorId(id.Value);return u==null?RedirectToAction("Login","Cuenta"):View(u);}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Index(string nombre,string apellidos,string telefono,string direccion){var id=HttpContext.Session.GetInt32("UsuarioId");if(!id.HasValue)return RedirectToAction("Login","Cuenta");if(string.IsNullOrWhiteSpace(nombre)||string.IsNullOrWhiteSpace(apellidos)||string.IsNullOrWhiteSpace(telefono)||string.IsNullOrWhiteSpace(direccion)||nombre.Length>90||apellidos.Length>120||telefono.Length>30||direccion.Length>350){TempData["Aviso"]="Revisá los datos del perfil.";return RedirectToAction(nameof(Index));}db.EditarPerfil(id.Value,nombre,apellidos,telefono,direccion);db.Log(id.Value,"Modificación","Datos personales");HttpContext.Session.SetString("Nombre",nombre+" "+apellidos);TempData["Aviso"]="Perfil actualizado.";return RedirectToAction(nameof(Index));}
}

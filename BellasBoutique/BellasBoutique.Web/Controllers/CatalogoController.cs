using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class CatalogoController(TiendaDb db):Controller
{
    public IActionResult Index(string? categoria){ViewBag.Categoria=categoria;return View(db.Productos(categoria:categoria));}
}

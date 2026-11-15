/* Responsable: Eduardo
   Catálogo de productos con filtro por categoría y añadir al carrito (módulo 3). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index() => View();
}

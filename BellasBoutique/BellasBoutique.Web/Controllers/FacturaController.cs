using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class FacturaController:Controller
{
    public IActionResult Detalle(){var f=HttpContext.Session.ObtenerFactura();return f==null?RedirectToAction("Index","Catalogo"):View(f);}
}

/* Responsable: Eduardo
   Gestión de productos: registrar, modificar, actualizar stock y dar de baja (módulo 3). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Areas.Administracion.Controllers;

[Area("Administracion")]
public class ProductosController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Formulario() => View();
}

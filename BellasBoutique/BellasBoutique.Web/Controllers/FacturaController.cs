/* Responsable: Joel
   Factura generada al confirmar la compra (módulos 4 y 5). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class FacturaController : Controller
{
    public IActionResult Detalle() => View();
}

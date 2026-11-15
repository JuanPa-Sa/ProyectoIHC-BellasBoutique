/* Responsable: Joel
   Carrito de compras y confirmación de la compra con método de pago (módulo 4). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class CarritoController : Controller
{
    public IActionResult Index() => View();

    public IActionResult ConfirmarCompra() => View();
}

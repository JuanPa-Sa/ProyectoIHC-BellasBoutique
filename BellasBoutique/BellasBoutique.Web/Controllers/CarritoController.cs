using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
using BellasBoutique.Web.Models.Demo;
namespace BellasBoutique.Web.Controllers;
public class CarritoController(TiendaDb db):Controller
{
    public IActionResult Index()=>View(HttpContext.Session.ObtenerCarrito());
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Agregar(int id,int cantidad=1)
    {var p=db.ProductoPorId(id);if(p==null||!p.Activo||p.Stock<1){TempData["Aviso"]="Producto no disponible.";return RedirectToAction("Index","Catalogo");}
      var items=HttpContext.Session.ObtenerCarrito();var i=items.FirstOrDefault(x=>x.ProductoId==id);var nueva=Math.Max(1,cantidad)+(i?.Cantidad??0);if(nueva>p.Stock){TempData["Aviso"]="No hay suficientes unidades.";return RedirectToAction("Index","Catalogo");}
      if(i==null)items.Add(new ItemDemo{ProductoId=p.Id,Nombre=p.Nombre,Categoria=p.Categoria,Precio=p.Precio,Cantidad=nueva,Imagen=p.Imagen});else i.Cantidad=nueva;
      HttpContext.Session.GuardarCarrito(items);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Quitar(int id){var items=HttpContext.Session.ObtenerCarrito();items.RemoveAll(x=>x.ProductoId==id);HttpContext.Session.GuardarCarrito(items);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Cantidad(int id,int cantidad){var items=HttpContext.Session.ObtenerCarrito();var item=items.FirstOrDefault(x=>x.ProductoId==id);if(item!=null){var p=db.ProductoPorId(id);if(p==null||p.Stock<=0)items.Remove(item);else item.Cantidad=Math.Clamp(cantidad,1,p.Stock);}HttpContext.Session.GuardarCarrito(items);return RedirectToAction(nameof(Index));}
    [HttpGet] public IActionResult ConfirmarCompra(){var items=HttpContext.Session.ObtenerCarrito();if(items.Count==0)return RedirectToAction(nameof(Index));return View(items);}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult ConfirmarCompra(string metodo,string direccion)
    {var items=HttpContext.Session.ObtenerCarrito();if(items.Count==0)return RedirectToAction(nameof(Index));
      if(!new[]{"Tarjeta","SINPE","Transferencia"}.Contains(metodo)||string.IsNullOrWhiteSpace(direccion)){TempData["Aviso"]="Elegí un pago y una dirección.";return RedirectToAction(nameof(ConfirmarCompra));}
      // Compra demostrativa: el stock sí cambia en SQL; no se almacena ningún pago ni venta.
      if(!db.DescontarStock(items)){TempData["Aviso"]="El stock cambió; revisá el carrito.";return RedirectToAction(nameof(Index));}
      var id=HttpContext.Session.GetInt32("UsuarioId");var u=id.HasValue?db.UsuarioPorId(id.Value):null;
      HttpContext.Session.GuardarFactura(new FacturaDemo{Numero=Random.Shared.Next(1000,9999),Cliente=u?.NombreCompleto??"Cliente de prueba",Correo=u?.Correo??"demo@bellasboutique.local",Detalle=items,Metodo=metodo,Direccion=direccion,Fecha=DateTime.Now});
      HttpContext.Session.GuardarCarrito(new());return RedirectToAction("Detalle","Factura");}
}

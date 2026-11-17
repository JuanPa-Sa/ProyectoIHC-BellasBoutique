using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
using BellasBoutique.Web.Models.Demo;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")]
public class ProductosController(TiendaDb db,IWebHostEnvironment entorno):Controller
{
    private bool Puede()=>new[]{"Administrador","Vendedor"}.Contains(HttpContext.Session.GetString("Rol"));
    public IActionResult Index(){if(!Puede())return Redirect("/Cuenta/Login");return View(db.Productos(true));}
    [HttpGet] public IActionResult Formulario(int? id){if(!Puede())return Redirect("/Cuenta/Login");return View(id.HasValue?db.ProductoPorId(id.Value):new ProductoDemo());}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Formulario(ProductoDemo p,IFormFile? imagenArchivo)
    {if(!Puede())return Redirect("/Cuenta/Login");if(string.IsNullOrWhiteSpace(p.Nombre)||p.Precio<0||p.Stock<0){ViewBag.Error="Revisá nombre, precio y stock.";return View(p);}
      if(imagenArchivo!=null&&imagenArchivo.Length>0){if(imagenArchivo.Length>3_000_000||!new[]{".png",".jpg",".jpeg",".webp"}.Contains(Path.GetExtension(imagenArchivo.FileName).ToLowerInvariant())){ViewBag.Error="Imagen inválida (JPG/PNG/WEBP, máximo 3 MB).";return View(p);}
        var nombre=Guid.NewGuid().ToString("N")+Path.GetExtension(imagenArchivo.FileName).ToLowerInvariant();var carpeta=Path.Combine(entorno.WebRootPath,"images","productos");Directory.CreateDirectory(carpeta);using var fs=System.IO.File.Create(Path.Combine(carpeta,nombre));await imagenArchivo.CopyToAsync(fs);p.Imagen="/images/productos/"+nombre;}
      else if(p.Id!=0&&string.IsNullOrWhiteSpace(p.Imagen))p.Imagen=db.ProductoPorId(p.Id)?.Imagen??"";
      db.GuardarProducto(p);TempData["Aviso"]="Producto guardado.";return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Baja(int id){if(!Puede())return Redirect("/Cuenta/Login");db.BajaProducto(id);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Stock(int id,int stock){if(!Puede())return Redirect("/Cuenta/Login");db.ActualizarStock(id,stock);return RedirectToAction(nameof(Index));}
}

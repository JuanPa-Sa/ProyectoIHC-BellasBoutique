using Microsoft.AspNetCore.Mvc;using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class FacturaController(TiendaDb db):Controller{public IActionResult Detalle(int? id){var f=db.Factura(HttpContext.Session.GetInt32("UsuarioId")??0,id);return f==null?RedirectToAction("Index","Catalogo"):View(f);}}
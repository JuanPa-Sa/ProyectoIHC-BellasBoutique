using Microsoft.AspNetCore.Mvc;using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")] public class BitacoraController(TiendaDb db):Controller{public IActionResult Index()=>View(db.Bitacora());}
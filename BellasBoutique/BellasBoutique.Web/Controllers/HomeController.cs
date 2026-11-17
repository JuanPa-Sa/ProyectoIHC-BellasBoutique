using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
namespace BellasBoutique.Web.Controllers;
public class HomeController(TiendaDb db):Controller
{
    public IActionResult Index()=>View(db.Productos().Take(4).ToList());
    public IActionResult Error()=>View();
}

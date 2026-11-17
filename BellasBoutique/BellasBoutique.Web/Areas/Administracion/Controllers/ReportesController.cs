using Microsoft.AspNetCore.Mvc;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")]
public class ReportesController(IWebHostEnvironment env):Controller
{
    public IActionResult Index()=>View();
    public IActionResult FinDelDia()=>View();
    public IActionResult Descargar(string tipo="diario")
    {if(!new[]{"diario","mensual","fin"}.Contains(tipo))tipo="diario";var ruta=Path.Combine(env.WebRootPath,"reportes","BellasBoutique_Reporte_"+tipo+".pdf");return PhysicalFile(ruta,"application/pdf","BellasBoutique_Reporte_"+tipo+".pdf");}
}

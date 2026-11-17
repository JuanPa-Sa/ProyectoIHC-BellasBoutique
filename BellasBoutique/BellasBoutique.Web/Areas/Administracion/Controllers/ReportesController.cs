using Microsoft.AspNetCore.Mvc;using BellasBoutique.Web.Data;using BellasBoutique.Web.Models.Demo;using BellasBoutique.Web.Services;
namespace BellasBoutique.Web.Areas.Administracion.Controllers;
[Area("Administracion")] public class ReportesController(TiendaDb db,IWebHostEnvironment env):Controller{
 List<VentaFila> Periodo(string tipo,DateTime fecha){var inicio=tipo=="mensual"?new DateTime(fecha.Year,fecha.Month,1):fecha.Date;var fin=tipo=="mensual"?inicio.AddMonths(1):inicio.AddDays(1);return db.Ventas().Where(v=>v.FechaReal>=inicio&&v.FechaReal<fin).ToList();}
 public IActionResult Index(string tipo="diario",DateTime? fecha=null){ViewBag.Tipo=tipo;ViewBag.Fecha=fecha??DateTime.Today;return View(Periodo(tipo,fecha??DateTime.Today));}
 public IActionResult FinDelDia(){ViewBag.Ventas=Periodo("diario",DateTime.Today);return View(db.Productos().Where(p=>p.Stock<=3).ToList());}

 // Genera el PDF del reporte (diario, mensual o cierre del día) con el diseño de la marca: ver Services/PdfReporte.cs
 public IActionResult Descargar(string tipo="diario",DateTime? fecha=null)
 {
    if(tipo is not ("diario" or "mensual" or "fin"))tipo="diario";
    var f=tipo=="fin"?DateTime.Today:fecha??DateTime.Today;
    var cr=new System.Globalization.CultureInfo("es-CR");

    var productos=Periodo(tipo,f)
        .GroupBy(x=>new{x.Producto,x.Categoria})
        .Select(g=>new{g.Key.Producto,g.Key.Categoria,Cantidad=g.Sum(x=>x.Cantidad),Ingresos=g.Sum(x=>x.Precio*x.Cantidad)})
        .OrderByDescending(x=>x.Ingresos).ToList();
    var ingresos=productos.Sum(x=>x.Ingresos);
    var unidades=productos.Sum(x=>x.Cantidad);
    string Colones(decimal monto)=>"₡"+monto.ToString("N0",cr);
    static string Mayuscula(string s)=>s.Length==0?s:char.ToUpper(s[0])+s[1..];

    var datos=new DatosReporte
    {
        Titulo=tipo switch{"mensual"=>"Reporte mensual","fin"=>"Cierre del día",_=>"Reporte diario"},
        Periodo=tipo=="mensual"?Mayuscula(f.ToString("MMMM 'de' yyyy",cr)):f.ToString("dd/MM/yyyy"),
        Generado=DateTime.Now
    };
    datos.Indicadores.Add(("Productos vendidos",unidades.ToString("N0",cr)));
    datos.Indicadores.Add(("Ingresos antes de IVA",Colones(ingresos)));
    datos.Indicadores.Add(("Productos distintos",productos.Count.ToString()));

    var detalle=new TablaReporte
    {
        Titulo="Detalle por producto",
        Encabezados=["Producto","Categoría","Cantidad","Ingresos","% de ingresos"],
        Anchos=[160,85,60,95,115],
        Derecha=[false,false,true,true,false],
        ColumnaBarra=4,
        Vacio="No hay ventas registradas en este periodo.",
        Total=["Total","",unidades.ToString("N0",cr),Colones(ingresos),""]
    };
    foreach(var p in productos)
    {
        var porcentaje=ingresos==0?0:(int)Math.Round(p.Ingresos*100/ingresos);
        detalle.Filas.Add([p.Producto,p.Categoria,p.Cantidad.ToString("N0",cr),Colones(p.Ingresos),porcentaje+"%"]);
    }
    datos.Tablas.Add(detalle);

    if(tipo=="fin")
    {
        var stockBajo=db.Productos().Where(p=>p.Stock<=3).OrderBy(p=>p.Stock).ToList();
        datos.Indicadores.Add(("Con stock bajo",stockBajo.Count.ToString()));
        var tabla=new TablaReporte
        {
            Titulo="Productos con stock bajo (3 unidades o menos)",
            Encabezados=["Código","Producto","Categoría","Stock","Estado"],
            Anchos=[70,185,100,65,95],
            Derecha=[false,false,false,true,false],
            ColumnaEstado=4,
            Vacio="Todos los productos tienen stock suficiente."
        };
        foreach(var p in stockBajo)tabla.Filas.Add([p.Codigo,p.Nombre,p.Categoria,p.Stock.ToString(),p.Stock==0?"Agotado":"Stock bajo"]);
        datos.Tablas.Add(tabla);
    }

    var rutaLogo=Path.Combine(env.WebRootPath,"images","logo-reporte.jpg");
    byte[]? logo=System.IO.File.Exists(rutaLogo)?System.IO.File.ReadAllBytes(rutaLogo):null;
    return File(PdfReporte.Crear(datos,logo),"application/pdf",$"BellasBoutique_{tipo}_{f:yyyyMMdd}.pdf");
 }
}

using System.Text.Json;
using BellasBoutique.Web.Models.Demo;
namespace BellasBoutique.Web.Data;
public static class SesionDemo
{
    public static List<ItemDemo> ObtenerCarrito(this ISession sesion)=>JsonSerializer.Deserialize<List<ItemDemo>>(sesion.GetString("Carrito")??"[]")??new();
    public static void GuardarCarrito(this ISession sesion,List<ItemDemo> items)=>sesion.SetString("Carrito",JsonSerializer.Serialize(items));
    public static FacturaDemo? ObtenerFactura(this ISession sesion)=>JsonSerializer.Deserialize<FacturaDemo>(sesion.GetString("Factura")??"null");
    public static void GuardarFactura(this ISession sesion,FacturaDemo factura)=>sesion.SetString("Factura",JsonSerializer.Serialize(factura));
}

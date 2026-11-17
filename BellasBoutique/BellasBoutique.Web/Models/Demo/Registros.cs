namespace BellasBoutique.Web.Models.Demo;
public record VentaFila(DateTime FechaReal,string Factura,string Producto,string Categoria,int Cantidad,decimal Precio){public string Fecha=>FechaReal.ToString("dd/MM/yyyy HH:mm");}
public record BitacoraFila(DateTime FechaReal,string Usuario,string Rol,string Accion,string Detalle){public string Fecha=>FechaReal.ToString("dd/MM/yyyy HH:mm:ss");}
public record PagoFila(string Factura,string Cliente,string Metodo,decimal Total,string Estado);
public record ChatFila(int Id,int EmisorId,string Texto,DateTime Fecha);
public record FeedbackFila(string Usuario,string Texto,DateTime Fecha,int? Puntaje);

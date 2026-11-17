namespace BellasBoutique.Web.Models.Demo;
public class ProductoDemo
{
    public int Id {get;set;}
    public string Codigo => $"P-{Id:0000}";
    public string Nombre {get;set;} = "";
    public string Descripcion {get;set;} = "";
    public decimal Precio {get;set;}
    public int Stock {get;set;}
    public string Categoria {get;set;} = "Ropa";
    public string Proveedor {get;set;} = "";
    public string Imagen {get;set;} = "";
    public bool Activo {get;set;} = true;
}
public class UsuarioDemo
{
    public int Id {get;set;}
    public string Codigo => $"U-{Id:0000}";
    public string Usuario {get;set;} = "";
    public string Cedula {get;set;} = "";
    public string Nombre {get;set;} = "";
    public string Apellidos {get;set;} = "";
    public string Correo {get;set;} = "";
    public string Telefono {get;set;} = "";
    public string Direccion {get;set;} = "";
    public string Contrasena {get;set;} = "";
    public string Rol {get;set;} = "Cliente";
    public bool Activo {get;set;} = true;
    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();
    public string Ultimo {get;set;} = "Sin acceso";
}
public class ItemDemo
{
    public int ProductoId {get;set;}
    public string Nombre {get;set;} = "";
    public string Categoria {get;set;} = "";
    public decimal Precio {get;set;}
    public int Cantidad {get;set;}
    public string Imagen {get;set;} = "";
}
public class FacturaDemo
{
    public int Numero {get;set;}
    public string Cliente {get;set;} = "";
    public string Correo {get;set;} = "";
    public string Metodo {get;set;} = "Tarjeta";
    public string Direccion {get;set;} = "";
    public DateTime Fecha {get;set;} = DateTime.Now;
    public List<ItemDemo> Detalle {get;set;} = new();
    public decimal Subtotal => Detalle.Sum(x => x.Precio*x.Cantidad);
    public decimal Impuesto => Math.Round(Subtotal*.13m,2,MidpointRounding.AwayFromZero);
    public decimal Total => Subtotal+Impuesto;
}

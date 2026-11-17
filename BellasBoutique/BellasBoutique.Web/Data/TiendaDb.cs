using System.Data;
using Microsoft.Data.SqlClient;
using BellasBoutique.Web.Models.Demo;
using BellasBoutique.Web.Services;

namespace BellasBoutique.Web.Data;
// ADO.NET básico: ninguna migración ni arquitectura extra. Ejecutar primero SQL/BellasBoutique_BD.sql.
public partial class TiendaDb(IConfiguration config)
{
    private SqlConnection Abrir() { var c=new SqlConnection(config.GetConnectionString("BellasBoutique")); c.Open(); return c; }
    private static SqlCommand Cmd(SqlConnection c,string sql,params (string,object?)[] ps)
    { var cmd=new SqlCommand(sql,c);foreach(var (k,v) in ps)cmd.Parameters.AddWithValue(k,v??DBNull.Value);return cmd; }
    private static ProductoDemo Producto(SqlDataReader r)=>new(){Id=r.GetInt32(r.GetOrdinal("Id")),Nombre=(string)r["Nombre"],Descripcion=(string)r["Descripcion"],Categoria=(string)r["Categoria"],Proveedor=(string)r["Proveedor"],Precio=(decimal)r["Precio"],Stock=(int)r["Stock"],Imagen=(string)r["Imagen"],Activo=(bool)r["Activo"]};
    private static UsuarioDemo Usuario(SqlDataReader r)=>new(){Id=(int)r["Id"],Usuario=(string)r["Usuario"],Cedula=(string)r["Cedula"],Nombre=(string)r["Nombre"],Apellidos=(string)r["Apellidos"],Correo=(string)r["Correo"],Telefono=(string)r["Telefono"],Direccion=(string)r["Direccion"],Contrasena=(string)r["Contrasena"],Rol=(string)r["Rol"],Activo=(bool)r["Activo"]};
    public List<ProductoDemo> Productos(bool incluirInactivos=false,string? categoria=null)
    {using var c=Abrir(); using var q=Cmd(c,"SELECT * FROM Productos WHERE (@todos=1 OR Activo=1) AND (@cat='' OR Categoria=@cat) ORDER BY Id",("@todos",incluirInactivos?1:0),("@cat",categoria??"")); using var r=q.ExecuteReader();var list=new List<ProductoDemo>();while(r.Read())list.Add(Producto(r));return list;}
    public ProductoDemo? ProductoPorId(int id){using var c=Abrir();using var q=Cmd(c,"SELECT * FROM Productos WHERE Id=@id",("@id",id));using var r=q.ExecuteReader();return r.Read()?Producto(r):null;}
    public void GuardarProducto(ProductoDemo p)
    {using var c=Abrir();using var q=Cmd(c,p.Id==0?"INSERT INTO Productos(Nombre,Descripcion,Categoria,Proveedor,Precio,Stock,Imagen,Activo) VALUES(@nombre,@desc,@cat,@prov,@precio,@stock,@imagen,1)":"UPDATE Productos SET Nombre=@nombre,Descripcion=@desc,Categoria=@cat,Proveedor=@prov,Precio=@precio,Stock=@stock,Imagen=@imagen WHERE Id=@id",("@id",p.Id),("@nombre",p.Nombre),("@desc",p.Descripcion),("@cat",p.Categoria),("@prov",p.Proveedor),("@precio",p.Precio),("@stock",p.Stock),("@imagen",p.Imagen));q.ExecuteNonQuery();}
    public void BajaProducto(int id){using var c=Abrir();using var q=Cmd(c,"UPDATE Productos SET Activo=0 WHERE Id=@id",("@id",id));q.ExecuteNonQuery();}
    public void ActualizarStock(int id,int stock){using var c=Abrir();using var q=Cmd(c,"UPDATE Productos SET Stock=@stock WHERE Id=@id",("@id",id),("@stock",Math.Max(0,stock)));q.ExecuteNonQuery();}
    public bool DescontarStock(List<ItemDemo> items)
    {using var c=Abrir();using var tx=c.BeginTransaction();try{foreach(var item in items){using var q=Cmd(c,"UPDATE Productos SET Stock=Stock-@cant WHERE Id=@id AND Activo=1 AND Stock>=@cant",("@id",item.ProductoId),("@cant",item.Cantidad));q.Transaction=tx;if(q.ExecuteNonQuery()!=1){tx.Rollback();return false;}}tx.Commit();return true;}catch{tx.Rollback();throw;}}
    public List<UsuarioDemo> Usuarios(){using var c=Abrir();using var q=Cmd(c,"SELECT u.*, (SELECT MAX(i.Fecha) FROM IntentosLogin i WHERE i.Exitoso=1 AND (i.Identificador=u.Correo OR i.Identificador=u.Usuario)) UltimoAcceso FROM Usuarios u ORDER BY Id");using var r=q.ExecuteReader();var list=new List<UsuarioDemo>();while(r.Read()){var u=Usuario(r);u.Ultimo=r["UltimoAcceso"] is DateTime f?f.ToString("dd/MM/yyyy HH:mm"):"Sin acceso";list.Add(u);}return list;}
    public UsuarioDemo? UsuarioPorId(int id){using var c=Abrir();using var q=Cmd(c,"SELECT * FROM Usuarios WHERE Id=@id",("@id",id));using var r=q.ExecuteReader();return r.Read()?Usuario(r):null;}
    public UsuarioDemo? Autenticar(string identificador,string contrasena){using var c=Abrir();using var q=Cmd(c,"SELECT * FROM Usuarios WHERE (Usuario=@usr OR Correo=@usr) AND Activo=1",("@usr",identificador));using var r=q.ExecuteReader();var u=r.Read()?Usuario(r):null;return u!=null&&CifradoService.Verificar(contrasena,u.Contrasena)?u:null;}
    public void Registrar(UsuarioDemo u)
    {using var c=Abrir();using var q=Cmd(c,"INSERT INTO Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol,Activo) VALUES(@u,@c,@n,@a,@e,@t,@d,@p,'Cliente',1)",("@u",u.Usuario),("@c",u.Cedula),("@n",u.Nombre),("@a",u.Apellidos),("@e",u.Correo),("@t",u.Telefono),("@d",u.Direccion),("@p",CifradoService.Hash(u.Contrasena)));q.ExecuteNonQuery();using var cl=Cmd(c,"INSERT Clientes(UsuarioId) SELECT Id FROM Usuarios WHERE Usuario=@u",("@u",u.Usuario));cl.ExecuteNonQuery();}
    public void EditarPerfil(int id,string nombre,string apellidos,string telefono,string direccion)
    {using var c=Abrir();using var q=Cmd(c,"UPDATE Usuarios SET Nombre=@n, Apellidos=@a, Telefono=@t, Direccion=@d WHERE Id=@id",("@id",id),("@n",nombre),("@a",apellidos),("@t",telefono),("@d",direccion));q.ExecuteNonQuery();}
    public void ModificarUsuario(int id,string rol,bool activo)
    {using var c=Abrir();using var q=Cmd(c,"UPDATE Usuarios SET Rol=@r,Activo=@a WHERE Id=@id",("@id",id),("@r",rol),("@a",activo));q.ExecuteNonQuery();using var cl=Cmd(c,"INSERT Clientes(UsuarioId) SELECT Id FROM Usuarios WHERE Id=@id AND Rol='Cliente' AND NOT EXISTS(SELECT 1 FROM Clientes WHERE UsuarioId=@id)",("@id",id));cl.ExecuteNonQuery();}
}

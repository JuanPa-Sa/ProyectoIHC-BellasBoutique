using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
using BellasBoutique.Web.Models.Demo;
using Microsoft.Data.SqlClient;
namespace BellasBoutique.Web.Controllers;
public class CuentaController(TiendaDb db):Controller
{
    [HttpGet] public IActionResult Login()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Login(string correo,string contrasena)
    {
        var u=db.Autenticar(correo??"",contrasena??"");
        if(u==null){ViewBag.Error="Usuario o contraseña incorrectos.";return View();}
        HttpContext.Session.SetInt32("UsuarioId",u.Id);
        HttpContext.Session.SetString("Rol",u.Rol);
        HttpContext.Session.SetString("Nombre",u.NombreCompleto);
        return u.Rol=="Cliente"?RedirectToAction("Index","Home"):Redirect("/Administracion/Home");
    }
    [HttpGet] public IActionResult Registro()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult Registro(string id,string cedula,string nombre,string apellidos,string correo,string telefono,string direccion,string contrasena)
    {
        if(new[]{id,cedula,nombre,apellidos,correo,contrasena}.Any(string.IsNullOrWhiteSpace)){ViewBag.Error="Completá los campos obligatorios.";return View();}
        try{db.Registrar(new UsuarioDemo{Usuario=id,Cedula=cedula,Nombre=nombre,Apellidos=apellidos,Correo=correo,Telefono=telefono??"",Direccion=direccion??"",Contrasena=contrasena});TempData["Aviso"]="Cuenta creada. Iniciá sesión.";return RedirectToAction(nameof(Login));}
        catch(SqlException){ViewBag.Error="El usuario, correo o cédula ya existe.";return View();}
    }
    [HttpGet] public IActionResult RecuperarContrasena()=>View();
    [HttpPost,ValidateAntiForgeryToken] public IActionResult RecuperarContrasena(string correo){ViewBag.Aviso="Simulación: se enviaría el correo de recuperación a "+correo+". No se cambió ninguna contraseña.";return View();}
    public IActionResult Salir(){HttpContext.Session.Clear();return RedirectToAction(nameof(Login));}
}

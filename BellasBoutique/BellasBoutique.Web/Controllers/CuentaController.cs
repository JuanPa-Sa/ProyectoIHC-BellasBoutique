using Microsoft.AspNetCore.Mvc;
using BellasBoutique.Web.Data;
using BellasBoutique.Web.Models.Demo;
using BellasBoutique.Web.Services;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
namespace BellasBoutique.Web.Controllers;
public class CuentaController(TiendaDb db,CorreoService correoService):Controller {
 [HttpGet] public IActionResult Login()=>View();
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Login(string correo,string contrasena){var u=db.Autenticar(correo??"",contrasena??"");db.Intento(correo??"",u!=null);if(u==null){ViewBag.Error="Usuario o contraseña incorrectos.";return View();}HttpContext.Session.Clear();HttpContext.Session.SetInt32("UsuarioId",u.Id);HttpContext.Session.SetString("UltimaActividad",DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());HttpContext.Session.SetString("Rol",u.Rol);HttpContext.Session.SetString("Nombre",u.NombreCompleto);HttpContext.Session.GuardarCarrito(db.Carrito(u.Id));return u.Rol=="Cliente"?RedirectToAction("Index","Home"):Redirect("/Administracion/Home");}
 [HttpGet] public IActionResult Registro()=>View();
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Registro(string id,string cedula,string nombre,string apellidos,string correo,string telefono,string direccion,string contrasena){if(new[]{id,cedula,nombre,apellidos,correo,telefono,direccion,contrasena}.Any(string.IsNullOrWhiteSpace)||!new EmailAddressAttribute().IsValid(correo)||!CifradoService.Valida(contrasena)||id.Length>80||cedula.Length>30||nombre.Length>90||apellidos.Length>120||correo.Length>150||telefono.Length>30||direccion.Length>350){ViewBag.Error="Completá los datos y usá un correo válido y una contraseña de 8 caracteres con mayúscula, minúscula, número y símbolo.";return View();}try{db.Registrar(new UsuarioDemo{Usuario=id,Cedula=cedula,Nombre=nombre,Apellidos=apellidos,Correo=correo,Telefono=telefono,Direccion=direccion,Contrasena=contrasena});var u=db.Autenticar(correo,contrasena);if(u!=null)db.Log(u.Id,"Alta","Registro de cliente");TempData["Aviso"]="Cuenta creada. Iniciá sesión.";return RedirectToAction(nameof(Login));}catch(SqlException e) when(e.Number is 2601 or 2627){ViewBag.Error="El usuario, correo o cédula ya existe.";return View();}}
 [HttpGet] public IActionResult RecuperarContrasena()=>View();
 [HttpPost,ValidateAntiForgeryToken] public IActionResult RecuperarContrasena(string correo){var u=db.Usuarios().FirstOrDefault(x=>x.Activo&&string.Equals(x.Correo,correo,StringComparison.OrdinalIgnoreCase));if(u!=null){var clave=CifradoService.Nueva();db.Clave(u.Id,clave);correoService.Enviar(u.Correo,"Recuperación BellasBoutique","Tu nueva contraseña temporal es: "+clave);db.Log(u.Id,"Modificación","Recuperación de contraseña");}ViewBag.Aviso="Si el correo está registrado, se ha generado una nueva contraseña. En modo local, consultá el buzón CorreosDemo.";return View();}
 public IActionResult Salir(){HttpContext.Session.Clear();return RedirectToAction(nameof(Login));}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Actividad()=>HttpContext.Session.GetInt32("UsuarioId")==null?Unauthorized():NoContent();
}
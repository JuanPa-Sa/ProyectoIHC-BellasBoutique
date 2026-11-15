/* Responsable: Juan Pablo
   Mi perfil: editar nombre, apellidos, dirección y teléfono; ID y correo no editables (módulo 7). */
using Microsoft.AspNetCore.Mvc;

namespace BellasBoutique.Web.Controllers;

public class PerfilController : Controller
{
    public IActionResult Index() => View();
}

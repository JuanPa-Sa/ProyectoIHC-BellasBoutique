var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BellasBoutique.Web.Data.TiendaDb>();
builder.Services.AddScoped<BellasBoutique.Web.Services.CorreoService>();

// Sesión: se cierra tras 5 minutos de inactividad (requerimiento 8a del enunciado)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
using(var scope=app.Services.CreateScope()){scope.ServiceProvider.GetRequiredService<BellasBoutique.Web.Data.TiendaDb>().MigrarClaves();}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();
// Área administrativa protegida con la sesión de demostración.
app.Use(async (context,next)=>{
    var uid=context.Session.GetInt32("UsuarioId");
    if(uid.HasValue){var ahora=DateTimeOffset.UtcNow.ToUnixTimeSeconds();var anterior=context.Session.GetString("UltimaActividad");if(long.TryParse(anterior,out var ultimo)&&ahora-ultimo>=300){context.Session.Clear();uid=null;}else if(context.Request.Path.Value?.EndsWith("/Mensajes",StringComparison.OrdinalIgnoreCase)!=true)context.Session.SetString("UltimaActividad",ahora.ToString());}
    if(uid.HasValue){var u=context.RequestServices.GetRequiredService<BellasBoutique.Web.Data.TiendaDb>().UsuarioPorId(uid.Value);if(u==null||!u.Activo)context.Session.Clear();else context.Session.SetString("Rol",u.Rol);}
    var protegida=new[]{"/Carrito","/Factura","/Perfil","/Soporte/Chat","/Soporte/Mensajes","/Soporte/Encuesta","/Soporte/Sugerencias"}.Any(p=>context.Request.Path.StartsWithSegments(p));
    if(protegida&&context.Session.GetInt32("UsuarioId")==null){if(context.Request.Path.Value?.EndsWith("/Mensajes")==true){context.Response.StatusCode=401;return;}context.Response.Redirect("/Cuenta/Login");return;}
    if((context.Request.Path.StartsWithSegments("/Carrito")||context.Request.Path.StartsWithSegments("/Factura"))&&context.Session.GetString("Rol")!="Cliente"){context.Response.Redirect("/Administracion/Home");return;}
    if(context.Request.Path.StartsWithSegments("/Administracion") &&
       context.Session.GetString("Rol") is not ("Administrador" or "Vendedor"))
    { context.Response.Redirect("/Cuenta/Login");return; }
    if((context.Request.Path.StartsWithSegments("/Administracion/Usuarios") || context.Request.Path.StartsWithSegments("/Administracion/Soporte/Feedback")) &&
       context.Session.GetString("Rol")!="Administrador")
    {context.Response.Redirect("/Administracion/Home");return;}
    await next();
});

// Vista Administración: /Administracion/{controlador}/{accion}
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Vista Cliente
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

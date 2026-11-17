var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BellasBoutique.Web.Data.TiendaDb>();

// Sesión: se cierra tras 5 minutos de inactividad (requerimiento 8a del enunciado)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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
    if(context.Request.Path.StartsWithSegments("/Administracion") &&
       context.Session.GetString("Rol") is not ("Administrador" or "Vendedor"))
    { context.Response.Redirect("/Cuenta/Login");return; }
    if(context.Request.Path.StartsWithSegments("/Administracion/Usuarios") &&
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

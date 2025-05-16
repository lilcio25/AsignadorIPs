using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using ASIGNADORIPS.Middlewares; // ✅ Asegúrate de tener este using

var builder = WebApplication.CreateBuilder(args);

// ✅ Servicios necesarios
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=equipos.db"));

// ✅ Sesión e HttpContext
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Alertas Automáticas
builder.Services.AddHostedService<ASIGNADORIPS.Services.AlertaLicenciasService>();

var app = builder.Build();

// ✅ Semilla de usuarios (admin y observador)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario { NombreUsuario = "admin", Contraseña = "admin123", Rol = "Administrador" });
        db.Usuarios.Add(new Usuario { NombreUsuario = "observador", Contraseña = "observador123", Rol = "Observador" });
        db.SaveChanges();
    }
}

// ✅ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Activar sesiones

app.UseMiddleware<HistorialMiddleware>(); // ✅ Middleware personalizado para registrar historial

app.UseAuthorization();

// ✅ Redirección manual si no hay sesión activa
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    // Permitir acceso sin sesión solo a login, logout y archivos estáticos
    if (path != null &&
        !path.StartsWith("/account/login") &&
        !path.StartsWith("/account/logout") &&
        !path.StartsWith("/assets") &&
        string.IsNullOrEmpty(context.Session.GetString("Rol")))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});

// ✅ Ruta por defecto: HomeController -> Index (una vez logueado)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

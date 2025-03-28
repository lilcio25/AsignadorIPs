using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Servicios
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=equipos.db"));

// ✅ Sesión y HTTP Context
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ Semilla de usuarios (solo si no hay ninguno)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Crea la DB si no existe

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
app.UseSession(); // Habilitar sesiones
app.UseAuthorization();

// ✅ Ruta por defecto: login de Account
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

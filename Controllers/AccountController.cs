using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;

namespace ASIGNADORIPS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Usuario usuario)
        {
            string hashedPassword = HashPassword(usuario.Contraseña);

            var user = _context.Usuarios
                .FirstOrDefault(u => u.NombreUsuario == usuario.NombreUsuario && u.Contraseña == hashedPassword);

            if (user != null)
            {
                HttpContext.Session.SetString("Usuario", user.NombreUsuario);
                HttpContext.Session.SetString("Rol", user.Rol);

                RegistrarHistorial($"Inicio de sesión exitoso");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        public IActionResult Logout()
        {
            RegistrarHistorial("Cierre de sesión");

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private string HashPassword(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // 🔐 Método para registrar el historial de acciones
        private void RegistrarHistorial(string accion)
        {
            var usuario = HttpContext.Session.GetString("Usuario") ?? "Desconocido";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no detectada";

            var historial = new HistorialAccion
            {
                Fecha = DateTime.Now,
                Usuario = usuario,
                Accion = accion,
                IP = ip
            };

            _context.HistorialAcciones.Add(historial);
            _context.SaveChanges();
        }
    }
}

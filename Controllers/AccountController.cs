using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
            // Hasheamos la contraseña ingresada
            string hashedPassword = HashPassword(usuario.Contraseña);

            var user = _context.Usuarios
                .FirstOrDefault(u => u.NombreUsuario == usuario.NombreUsuario && u.Contraseña == hashedPassword);

            if (user != null)
            {
                HttpContext.Session.SetString("Usuario", user.NombreUsuario);
                HttpContext.Session.SetString("Rol", user.Rol); // "administrador" u "observador"
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        public IActionResult Logout()
        {
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
    }
}

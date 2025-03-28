using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ASIGNADORIPS.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null) return RedirectToAction("Login", "Account");
            if (rol.ToLower() != "administrador") return RedirectToAction("Index", "Equipo");

            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        [HttpPost]
        public IActionResult CreateInline([FromBody] Usuario usuario)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.ToLower() != "administrador") return Unauthorized();

            if (ModelState.IsValid)
            {
                if (_context.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                    return BadRequest("El nombre de usuario ya existe.");

                usuario.Contraseña = HashPassword(usuario.Contraseña);
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult EditInline([FromBody] Usuario usuario)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.ToLower() != "administrador") return Unauthorized();

            var userDb = _context.Usuarios.Find(usuario.Id);
            if (userDb == null) return NotFound();

            userDb.NombreUsuario = usuario.NombreUsuario;

            if (!string.IsNullOrEmpty(usuario.Contraseña))
                userDb.Contraseña = HashPassword(usuario.Contraseña);

            userDb.Rol = usuario.Rol;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.ToLower() != "administrador") return Unauthorized();

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            return Ok();
        }

        private string HashPassword(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

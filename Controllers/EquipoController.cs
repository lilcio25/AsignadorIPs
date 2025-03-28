using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ASIGNADORIPS.Controllers
{
    public class EquipoController : Controller
    {
        private readonly AppDbContext _context;

        public EquipoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Equipo
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol") == null)
                return RedirectToAction("Login", "Account");

            var equipos = _context.Equipos.ToList();
            return View(equipos);
        }

        // GET: /Equipo/Create
        public IActionResult Create()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null) return RedirectToAction("Login", "Account");
            if (rol.ToLower() != "administrador") return RedirectToAction("Index");

            return View();
        }

        // POST: /Equipo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Equipo equipo)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null) return RedirectToAction("Login", "Account");
            if (rol.ToLower() != "administrador") return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                _context.Equipos.Add(equipo);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(equipo);
        }

        [HttpPost]
        public IActionResult UpdateInline([FromBody] Equipo equipo)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.ToLower() != "administrador") return Unauthorized();

            if (equipo == null || equipo.Id == 0) return BadRequest("Datos inválidos.");

            var equipoExistente = _context.Equipos.Find(equipo.Id);
            if (equipoExistente == null) return NotFound();

            equipoExistente.Piso = equipo.Piso;
            equipoExistente.NombreEquipo = equipo.NombreEquipo;
            equipoExistente.CodigoInventario = equipo.CodigoInventario;
            equipoExistente.Marca = equipo.Marca;
            equipoExistente.Procesador = equipo.Procesador;
            equipoExistente.RAM = equipo.RAM;
            equipoExistente.Disco = equipo.Disco;
            equipoExistente.Windows = equipo.Windows;
            equipoExistente.CodigoMonitor = equipo.CodigoMonitor;
            equipoExistente.SegmentoRed = equipo.SegmentoRed;
            equipoExistente.IP = equipo.IP;
            equipoExistente.Office = equipo.Office;
            equipoExistente.UsuarioAsignado = equipo.UsuarioAsignado;
            equipoExistente.NombreUsuario = equipo.NombreUsuario;
            equipoExistente.Anydesk = equipo.Anydesk;

            _context.SaveChanges();
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteInline(int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.ToLower() != "administrador") return Unauthorized();

            var equipo = _context.Equipos.Find(id);
            if (equipo == null) return NotFound();

            _context.Equipos.Remove(equipo);
            _context.SaveChanges();
            return Ok(new { success = true });
        }

        [HttpGet]
        public IActionResult ObtenerIpDisponible(int piso)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null) return Unauthorized();

            string segmento = piso switch
            {
                int n when new[] { 1, 5, 6, 7 }.Contains(n) => "200",
                int n when new[] { 2, 8, 9 }.Contains(n) => "190",
                int n when new[] { 3, 4, 10, 11 }.Contains(n) => "180",
                _ => null
            };

            if (segmento == null) return BadRequest("Piso inválido");

            var usadas = _context.Equipos
                .Where(e => e.SegmentoRed == segmento)
                .Select(e => e.IP)
                .ToList();

            for (int i = 10; i <= 254; i++)
            {
                string ip = $"192.9.{segmento}.{i}";
                if (!usadas.Contains(ip))
                {
                    return Json(new { ip });
                }
            }

            return BadRequest("No hay IPs disponibles en este segmento");
        }
    }
}

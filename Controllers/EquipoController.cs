using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace ASIGNADORIPS.Controllers
{
    public class EquipoController : Controller
    {
        private readonly AppDbContext _context;

        public EquipoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null)
                return RedirectToAction("Login", "Account");

            RegistrarHistorial("Accedió a la vista de equipos");
            var equipos = _context.Equipos.ToList();
            return View(equipos);
        }

        public IActionResult Create()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null || rol.Trim().ToLower() != "administrador")
                return RedirectToAction("Index");

            RegistrarHistorial("Accedió a la vista de creación de equipos");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Equipo equipo)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null || rol.Trim().ToLower() != "administrador")
                return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(equipo.NombreEquipo) ||
                string.IsNullOrWhiteSpace(equipo.CodigoInventario) ||
                string.IsNullOrWhiteSpace(equipo.SegmentoRed) ||
                string.IsNullOrWhiteSpace(equipo.IP))
            {
                ModelState.AddModelError("", "Por favor complete todos los campos obligatorios.");
                return View(equipo);
            }

            string ipCompleta = $"192.9.{equipo.SegmentoRed}.{equipo.IP}";

            bool ipDuplicada = _context.Equipos.Any(e =>
                e.SegmentoRed == equipo.SegmentoRed && e.IP == ipCompleta);

            if (ipDuplicada)
            {
                ModelState.AddModelError("IP", "Esta IP ya se encuentra registrada en este segmento.");
                return View(equipo);
            }

            equipo.IP = ipCompleta;
            equipo.Marca ??= "---";
            equipo.Procesador ??= "---";
            equipo.RAM ??= "---";
            equipo.Disco ??= "---";
            equipo.Windows ??= "---";
            equipo.CodigoMonitor ??= "---";
            equipo.Office ??= "---";
            equipo.UsuarioAsignado ??= "---";
            equipo.NombreUsuario ??= "---";
            equipo.Anydesk ??= "---";

            _context.Equipos.Add(equipo);
            _context.SaveChanges();

            RegistrarHistorial($"Registró equipo manualmente: {equipo.NombreEquipo} - IP {equipo.IP}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CargarDesdeExcel(IFormFile archivoExcel)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null || rol.Trim().ToLower() != "administrador")
                return RedirectToAction("Index");

            if (archivoExcel == null || archivoExcel.Length == 0)
                return RedirectToAction("Create");

            int insertados = 0;

            using var stream = new MemoryStream();
            archivoExcel.CopyTo(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var hoja = package.Workbook.Worksheets.FirstOrDefault();
            if (hoja == null)
                return RedirectToAction("Create");

            for (int row = 2; row <= hoja.Dimension.End.Row; row++)
            {
                var piso = int.TryParse(hoja.Cells[row, 1].Text, out int p) ? p : 0;
                var segmento = hoja.Cells[row, 10].Text ?? "---";
                var ultimoIP = hoja.Cells[row, 11].Text ?? "---";
                var ipCompleta = $"192.9.{segmento}.{ultimoIP}";

                var equipo = new Equipo
                {
                    Piso = piso,
                    NombreEquipo = hoja.Cells[row, 2].Text ?? "---",
                    CodigoInventario = hoja.Cells[row, 3].Text ?? "---",
                    Marca = hoja.Cells[row, 4].Text ?? "---",
                    Procesador = hoja.Cells[row, 5].Text ?? "---",
                    RAM = hoja.Cells[row, 6].Text ?? "---",
                    Disco = hoja.Cells[row, 7].Text ?? "---",
                    Windows = hoja.Cells[row, 8].Text ?? "---",
                    CodigoMonitor = hoja.Cells[row, 9].Text ?? "---",
                    SegmentoRed = segmento,
                    IP = ipCompleta,
                    Office = hoja.Cells[row, 12].Text ?? "---",
                    UsuarioAsignado = hoja.Cells[row, 13].Text ?? "---",
                    NombreUsuario = hoja.Cells[row, 14].Text ?? "---",
                    Anydesk = hoja.Cells[row, 15].Text ?? "---"
                };

                bool ipDuplicada = _context.Equipos.Any(e =>
                    e.SegmentoRed == segmento && e.IP == ipCompleta);

                if (!ipDuplicada)
                {
                    _context.Equipos.Add(equipo);
                    insertados++;
                }
            }

            _context.SaveChanges();
            RegistrarHistorial($"Cargó {insertados} equipos desde Excel");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateInline([FromBody] Equipo equipo)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.Trim().ToLower() != "administrador")
                return Unauthorized();

            if (equipo == null || equipo.Id == 0)
                return BadRequest("Datos inválidos.");

            var equipoDb = _context.Equipos.Find(equipo.Id);
            if (equipoDb == null)
                return NotFound();

            string ipCompleta = $"192.9.{equipo.SegmentoRed}.{equipo.IP}";

            if (_context.Equipos.Any(e =>
                e.Id != equipo.Id &&
                e.SegmentoRed == equipo.SegmentoRed &&
                e.IP == ipCompleta))
            {
                return BadRequest("Esta IP ya está registrada en este segmento.");
            }

            equipoDb.Piso = equipo.Piso;
            equipoDb.NombreEquipo = equipo.NombreEquipo ?? "---";
            equipoDb.CodigoInventario = equipo.CodigoInventario ?? "---";
            equipoDb.Marca = equipo.Marca ?? "---";
            equipoDb.Procesador = equipo.Procesador ?? "---";
            equipoDb.RAM = equipo.RAM ?? "---";
            equipoDb.Disco = equipo.Disco ?? "---";
            equipoDb.Windows = equipo.Windows ?? "---";
            equipoDb.CodigoMonitor = equipo.CodigoMonitor ?? "---";
            equipoDb.SegmentoRed = equipo.SegmentoRed ?? "---";
            equipoDb.IP = ipCompleta;
            equipoDb.Office = equipo.Office ?? "---";
            equipoDb.UsuarioAsignado = equipo.UsuarioAsignado ?? "---";
            equipoDb.NombreUsuario = equipo.NombreUsuario ?? "---";
            equipoDb.Anydesk = equipo.Anydesk ?? "---";

            _context.SaveChanges();
            RegistrarHistorial($"Editó equipo: {equipo.NombreEquipo} - IP {ipCompleta}");
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteInline([FromBody] int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.Trim().ToLower() != "administrador")
                return Unauthorized();

            var equipo = _context.Equipos.Find(id);
            if (equipo == null)
                return NotFound();

            _context.Equipos.Remove(equipo);
            _context.SaveChanges();

            RegistrarHistorial($"Eliminó equipo: {equipo.NombreEquipo} - IP {equipo.IP}");
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult EliminarMultiples([FromBody] List<int> ids)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol?.Trim().ToLower() != "administrador")
                return Unauthorized();

            if (ids == null || !ids.Any())
                return BadRequest("No se enviaron IDs");

            var eliminados = 0;

            foreach (var id in ids)
            {
                var equipo = _context.Equipos.Find(id);
                if (equipo != null)
                {
                    _context.Equipos.Remove(equipo);
                    eliminados++;
                }
            }

            _context.SaveChanges();
            RegistrarHistorial($"Eliminó {eliminados} equipos mediante selección múltiple");
            return Ok();
        }

        [HttpGet]
        public IActionResult ObtenerIpsDisponibles(int piso)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol == null)
                return Unauthorized();

            string segmento = piso switch
            {
                int n when new[] { 1, 5, 6, 7 }.Contains(n) => "200",
                int n when new[] { 2, 8, 9 }.Contains(n) => "190",
                int n when new[] { 3, 4, 10, 11 }.Contains(n) => "180",
                _ => null
            };

            if (segmento == null)
                return BadRequest("Piso inválido");

            var usadas = _context.Equipos
                .Where(e => e.SegmentoRed == segmento)
                .AsEnumerable()
                .Select(e => e.IP.Split('.').Last())
                .ToHashSet();

            var disponibles = new List<string>();
            for (int i = 1; i <= 255; i++)
            {
                string ultimo = i.ToString();
                if (!usadas.Contains(ultimo))
                    disponibles.Add(ultimo);
            }

            return Json(disponibles);
        }

        [HttpPost]
        public JsonResult ValidarDuplicados([FromBody] Equipo equipo)
        {
            string ipCompleta = $"192.9.{equipo.SegmentoRed}.{equipo.IP}";

            bool duplicado = _context.Equipos.Any(e =>
                e.SegmentoRed == equipo.SegmentoRed && e.IP == ipCompleta);

            return Json(new
            {
                duplicado,
                mensaje = duplicado ? "Esta IP ya se encuentra registrada." : ""
            });
        }

        [HttpGet]
        public IActionResult EscanearRed()
        {
            var equipos = _context.Equipos
                .Select(e => new
                {
                    ip = e.IP,
                    usuario = e.UsuarioAsignado,
                    piso = e.Piso,
                    segmento = e.SegmentoRed
                })
                .ToList();

            RegistrarHistorial("Escaneó la red local para visualizar IPs activas");
            return Json(equipos);
        }

        // 🔐 Reutilizable para registrar historial
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

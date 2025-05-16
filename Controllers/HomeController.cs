using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System;
using System.Linq;

namespace ASIGNADORIPS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            RegistrarHistorial("Accedió al dashboard principal");
            return View();
        }

        [HttpGet("/api/dashboard/kpis")]
        public IActionResult ObtenerKpis()
        {
            RegistrarHistorial("Consultó los KPIs del dashboard");

            var hoy = DateTime.Today;
            var proximas = hoy.AddDays(30);

            var totalLicencias = _context.Softwares.Sum(s => s.CantidadLicencias);
            var disponibles = _context.Softwares.Sum(s => s.LicenciasDisponibles);
            var porVencer = _context.Softwares.Count(s =>
                s.FechaExpiracion >= hoy && s.FechaExpiracion <= proximas
            );
            var funcionariosConAsignaciones = _context.UsuarioSoftwares
                .Select(us => us.PersonalId)
                .Distinct()
                .Count();

            var costoTotal = _context.Softwares
                .AsEnumerable()
                .Sum(s => Convert.ToDouble(s.CostoTotal));

            var proyeccionTresAnios = costoTotal * 3;

            return Json(new
            {
                totalLicencias,
                disponibles,
                porVencer,
                funcionariosConAsignaciones,
                costoTotal,
                proyeccionTresAnios
            });
        }

        [HttpGet("/api/dashboard/uso-licencias")]
        public IActionResult UsoLicencias()
        {
            RegistrarHistorial("Consultó el uso de licencias por software");

            var data = _context.Softwares
                .Select(s => new
                {
                    nombre = s.Nombre,
                    asignadas = _context.UsuarioSoftwares.Count(us => us.SoftwareId == s.Id),
                    disponibles = s.LicenciasDisponibles,
                    costoTotal = s.CostoTotal,
                    total = s.CantidadLicencias
                })
                .ToList();

            return Json(data);
        }

        [HttpGet("/api/dashboard/asignaciones-personal")]
        public IActionResult AsignacionesPorPersona()
        {
            RegistrarHistorial("Consultó las asignaciones por persona");

            var data = _context.Personal
                .Select(p => new
                {
                    nombre = p.NombreCompleto,
                    cantidad = _context.UsuarioSoftwares.Count(us => us.PersonalId == p.Id)
                })
                .Where(x => x.cantidad > 0)
                .ToList();

            return Json(data);
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

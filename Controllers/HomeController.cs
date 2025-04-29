using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
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
            return View();
        }

        [HttpGet("/api/dashboard/kpis")]
        public IActionResult ObtenerKpis()
        {
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
    }
}

using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using ASIGNADORIPS.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace ASIGNADORIPS.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly AppDbContext _context;

        public SoftwareController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult VistaSoft()
        {
            RegistrarHistorial("Ingresó a la vista de licencias de software");
            var softwares = _context.Softwares.ToList();
            return View(softwares);
        }

        [HttpPost]
        public IActionResult AgregarSoftware([FromBody] Software nuevo)
        {
            if (nuevo == null)
                return BadRequest("Datos inválidos");

            nuevo.LicenciasDisponibles = nuevo.CantidadLicencias;

            if (!nuevo.Notificar)
            {
                nuevo.DiasAntesNotificar = 0;
                nuevo.CorreosNotificacion = "";
            }

            _context.Softwares.Add(nuevo);
            _context.SaveChanges();

            RegistrarHistorial($"Agregó una nueva licencia: {nuevo.Nombre}");
            return Ok();
        }

        [HttpPost]
        public IActionResult EditarSoftware([FromBody] Software softwareEditado)
        {
            if (softwareEditado == null)
                return BadRequest("Datos inválidos");

            var software = _context.Softwares.FirstOrDefault(s => s.Id == softwareEditado.Id);
            if (software == null)
                return NotFound();

            software.Nombre = softwareEditado.Nombre;
            software.Descripcion = softwareEditado.Descripcion;
            software.OrdenCompra = softwareEditado.OrdenCompra;
            software.CantidadLicencias = softwareEditado.CantidadLicencias;
            software.LicenciasDisponibles = softwareEditado.LicenciasDisponibles;
            software.CostoTotal = softwareEditado.CostoTotal;
            software.FechaAdquisicion = softwareEditado.FechaAdquisicion;
            software.FechaExpiracion = softwareEditado.FechaExpiracion;

            _context.SaveChanges();

            RegistrarHistorial($"Editó la licencia: {softwareEditado.Nombre}");
            return Ok();
        }

        [HttpPost("/Software/EliminarLicencia/{id}")]
        public IActionResult EliminarLicencia(int id)
        {
            var software = _context.Softwares.FirstOrDefault(s => s.Id == id);
            if (software == null)
                return NotFound();

            var asignaciones = _context.UsuarioSoftwares.Where(us => us.SoftwareId == id).ToList();
            _context.UsuarioSoftwares.RemoveRange(asignaciones);

            _context.Softwares.Remove(software);
            _context.SaveChanges();

            RegistrarHistorial($"Eliminó la licencia: {software.Nombre}");
            return Ok();
        }

        [HttpPost]
        public IActionResult ActualizarAlertas([FromBody] List<SoftwareAlertaDTO> data)
        {
            if (data == null || !data.Any())
                return BadRequest("No se recibieron datos válidos.");

            foreach (var item in data)
            {
                var software = _context.Softwares.FirstOrDefault(s => s.Id == item.Id);
                if (software != null)
                {
                    software.Notificar = item.Notificar;
                    software.DiasAntesNotificar = item.Notificar ? item.DiasAntes : 0;
                    software.CorreosNotificacion = item.Notificar ? (item.Correos?.Trim() ?? "") : "";
                }
            }

            _context.SaveChanges();
            RegistrarHistorial("Actualizó configuraciones de alertas de licencias");
            return Ok();
        }

        [HttpGet]
        public IActionResult ObtenerAlertas()
        {
            var alertas = _context.Softwares
                .Select(s => new
                {
                    id = s.Id,
                    nombre = s.Nombre,
                    notificar = s.Notificar,
                    diasAntesNotificar = s.DiasAntesNotificar,
                    correosNotificacion = s.CorreosNotificacion
                })
                .ToList();

            return Json(alertas);
        }

        public IActionResult Historial()
        {
            RegistrarHistorial("Ingresó al historial de acciones");
            var historial = _context.HistorialAcciones.ToList();
            return View(historial);
        }

        public IActionResult UsuariosSoft()
        {
            RegistrarHistorial("Ingresó a la vista de asociación de licencias");
            var viewModel = new AsociacionLicenciasViewModel
            {
                Personal = _context.Personal.ToList(),
                Softwares = _context.Softwares.ToList(),
                Asignaciones = _context.UsuarioSoftwares.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AsociarLicencias(int personalId, List<int> softwareIds)
        {
            foreach (var softwareId in softwareIds)
            {
                bool yaExiste = _context.UsuarioSoftwares
                    .Any(us => us.PersonalId == personalId && us.SoftwareId == softwareId);

                var software = _context.Softwares.FirstOrDefault(s => s.Id == softwareId);

                if (!yaExiste && software != null && software.LicenciasDisponibles > 0)
                {
                    _context.UsuarioSoftwares.Add(new UsuarioSoftware
                    {
                        PersonalId = personalId,
                        SoftwareId = softwareId
                    });

                    software.LicenciasDisponibles--;
                }
            }

            _context.SaveChanges();
            RegistrarHistorial($"Asoció licencias al personal con ID {personalId}");
            return RedirectToAction("UsuariosSoft");
        }

        [HttpPost]
        public IActionResult Desasociar([FromBody] DesasociacionDTO input)
        {
            var asignacion = _context.UsuarioSoftwares
                .FirstOrDefault(a => a.PersonalId == input.PersonalId && a.SoftwareId == input.SoftwareId);

            if (asignacion != null)
            {
                _context.UsuarioSoftwares.Remove(asignacion);

                var software = _context.Softwares.FirstOrDefault(s => s.Id == input.SoftwareId);
                if (software != null)
                    software.LicenciasDisponibles++;

                _context.SaveChanges();
                RegistrarHistorial($"Desasoció la licencia {software.Nombre} del personal ID {input.PersonalId}");
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult LiberarLicenciasPorPersonal(int personalId)
        {
            var asignaciones = _context.UsuarioSoftwares
                .Where(us => us.PersonalId == personalId)
                .ToList();

            foreach (var asignacion in asignaciones)
            {
                var software = _context.Softwares.FirstOrDefault(s => s.Id == asignacion.SoftwareId);
                if (software != null)
                {
                    software.LicenciasDisponibles++;
                }

                _context.UsuarioSoftwares.Remove(asignacion);
            }

            _context.SaveChanges();
            RegistrarHistorial($"Liberó todas las licencias asociadas al personal ID {personalId}");
            return Ok();
        }

        // 🔐 Registro de historial centralizado
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

        public class SoftwareAlertaDTO
        {
            public int Id { get; set; }
            public bool Notificar { get; set; }
            public int DiasAntes { get; set; }
            public string Correos { get; set; }
        }

        public class DesasociacionDTO
        {
            public int PersonalId { get; set; }
            public int SoftwareId { get; set; }
        }
    }
}

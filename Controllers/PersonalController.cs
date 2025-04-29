using Microsoft.AspNetCore.Mvc;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ASIGNADORIPS.Controllers
{
    public class PersonalController : Controller
    {
        private readonly AppDbContext _context;

        public PersonalController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var personal = _context.Personal.ToList();
            return View("~/Views/Usuario/personal.cshtml", personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Personal persona)
        {
            if (ModelState.IsValid)
            {
                _context.Personal.Add(persona);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/Usuario/personal.cshtml", _context.Personal.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CargarExcel(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Seleccione un archivo válido.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    archivo.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var sheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (sheet == null)
                        {
                            TempData["Error"] = "El archivo no contiene hojas.";
                            return RedirectToAction("Index");
                        }

                        for (int row = 2; row <= sheet.Dimension.End.Row; row++)
                        {
                            var codigo = sheet.Cells[row, 1].Text.Trim();
                            var nombre = sheet.Cells[row, 2].Text.Trim();
                            var correo = sheet.Cells[row, 3].Text.Trim();

                            if (!string.IsNullOrEmpty(codigo) && !string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(correo))
                            {
                                bool existe = _context.Personal.Any(p => p.CodigoFuncionario == codigo);
                                if (!existe)
                                {
                                    _context.Personal.Add(new Personal
                                    {
                                        CodigoFuncionario = codigo,
                                        NombreCompleto = nombre,
                                        CorreoFuncionario = correo
                                    });
                                }
                            }
                        }

                        _context.SaveChanges();
                        TempData["Exito"] = "Carga masiva exitosa.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar el archivo: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EditarInline([FromBody] Personal p)
        {
            var existente = _context.Personal.FirstOrDefault(x => x.Id == p.Id);
            if (existente == null) return NotFound();

            existente.CodigoFuncionario = p.CodigoFuncionario;
            existente.NombreCompleto = p.NombreCompleto;
            existente.CorreoFuncionario = p.CorreoFuncionario;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult EliminarInline([FromBody] JsonElement body)
        {
            if (!body.TryGetProperty("id", out var idElement) || !idElement.TryGetInt32(out var id))
                return BadRequest("Id no válido");

            return EliminarPersonalYLicencias(id);
        }

        [HttpPost]
        public IActionResult EliminarMultiples([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("No se enviaron IDs");

            foreach (var id in ids)
            {
                EliminarPersonalYLicencias(id);
            }

            _context.SaveChanges();
            return Ok();
        }

        private IActionResult EliminarPersonalYLicencias(int id)
        {
            var persona = _context.Personal.FirstOrDefault(p => p.Id == id);
            if (persona == null) return NotFound();

            var asignaciones = _context.UsuarioSoftwares.Where(us => us.PersonalId == id).ToList();
            foreach (var asignacion in asignaciones)
            {
                var software = _context.Softwares.FirstOrDefault(s => s.Id == asignacion.SoftwareId);
                if (software != null)
                    software.LicenciasDisponibles++;

                _context.UsuarioSoftwares.Remove(asignacion);
            }

            _context.Personal.Remove(persona);
            return Ok(); // La llamada a SaveChanges se hace fuera en EliminarMultiples
        }
    }
}

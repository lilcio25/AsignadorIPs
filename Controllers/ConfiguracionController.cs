using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ASIGNADORIPS.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly AppDbContext _context;

        public ConfiguracionController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Correo()
        {
            var config = _context.ConfiguracionCorreo.FirstOrDefault() ?? new ConfiguracionCorreo();
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Correo(ConfiguracionCorreo model)
        {
            var existente = _context.ConfiguracionCorreo.FirstOrDefault();

            if (existente == null)
            {
                _context.ConfiguracionCorreo.Add(model);
            }
            else
            {
                existente.Remitente = model.Remitente;
                existente.HostSMTP = model.HostSMTP;
                existente.PuertoSMTP = model.PuertoSMTP;
                existente.UsuarioSMTP = model.UsuarioSMTP;
                existente.ClaveSMTP = model.ClaveSMTP;
                existente.UsarSSL = model.UsarSSL;
            }

            _context.SaveChanges();
            ViewBag.Mensaje = "Configuración guardada correctamente";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarPrueba(string destinatario)
        {
            var config = _context.ConfiguracionCorreo.FirstOrDefault();
            if (config == null)
            {
                ViewBag.Error = "No hay configuración de correo guardada.";
                return View("Correo", new ConfiguracionCorreo());
            }

            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(config.Remitente),
                    Subject = "[PRUEBA] Correo de prueba exitoso",
                    Body = "Este es un correo de prueba enviado correctamente desde el sistema."
                };
                mail.To.Add(destinatario);

                using var smtp = new SmtpClient
                {
                    Host = config.HostSMTP,
                    Port = config.PuertoSMTP,
                    EnableSsl = config.UsarSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(config.UsuarioSMTP, config.ClaveSMTP)
                };

                await smtp.SendMailAsync(mail);
                ViewBag.Mensaje = $"Correo de prueba enviado a {destinatario}";
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Error al enviar correo de prueba: " + ex.Message;
            }

            return View("Correo", config);
        }
    }
}

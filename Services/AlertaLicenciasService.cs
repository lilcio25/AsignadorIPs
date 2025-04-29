using ASIGNADORIPS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace ASIGNADORIPS.Services
{
    public class AlertaLicenciasService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertaLicenciasService> _logger;

        public AlertaLicenciasService(IServiceProvider serviceProvider, ILogger<AlertaLicenciasService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var hoy = DateTime.Today;

                    var config = await db.ConfiguracionCorreo.FirstOrDefaultAsync();
                    if (config == null)
                    {
                        _logger.LogWarning("No hay configuración SMTP cargada. No se enviarán alertas.");
                        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        continue;
                    }

                    var softwares = await db.Softwares
                        .Where(s => s.Notificar && s.DiasAntesNotificar > 0 && !string.IsNullOrWhiteSpace(s.CorreosNotificacion))
                        .ToListAsync();

                    foreach (var s in softwares)
                    {
                        var diasRestantes = (s.FechaExpiracion - hoy).TotalDays;

                        if (diasRestantes == s.DiasAntesNotificar)
                        {
                            try
                            {
                                var destinatarios = s.CorreosNotificacion
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                                foreach (var correo in destinatarios)
                                {
                                    var mail = new MailMessage
                                    {
                                        From = new MailAddress(config.Remitente),
                                        Subject = $"[Alerta] Licencia de {s.Nombre} por expirar",
                                        Body = $"La licencia del software '{s.Nombre}' expira el {s.FechaExpiracion:dd/MM/yyyy}. Por favor revisar y tomar las medidas pertinentes"
                                    };
                                    mail.To.Add(correo);

                                    using var smtp = new SmtpClient(config.HostSMTP)
                                    {
                                        Port = config.PuertoSMTP,
                                        Credentials = new System.Net.NetworkCredential(config.UsuarioSMTP, config.ClaveSMTP),
                                        EnableSsl = config.UsarSSL
                                    };

                                    await smtp.SendMailAsync(mail);
                                    _logger.LogInformation($"Correo enviado para {s.Nombre} a {correo}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error al enviar correo para {s.Nombre}: {ex.Message} {(ex.InnerException != null ? " | " + ex.InnerException.Message : "")}");
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

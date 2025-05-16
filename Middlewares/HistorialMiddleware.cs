using Microsoft.AspNetCore.Http;
using ASIGNADORIPS.Data;
using ASIGNADORIPS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASIGNADORIPS.Middlewares
{
    public class HistorialMiddleware
    {
        private readonly RequestDelegate _next;

        public HistorialMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Excluir rutas irrelevantes
            if (!string.IsNullOrEmpty(path) &&
                !path.StartsWith("/assets") &&
                !path.StartsWith("/favicon") &&
                !path.StartsWith("/account/login") &&
                !path.StartsWith("/account/logout") &&
                !path.Contains(".js") &&
                !path.Contains(".css") &&
                !path.Contains(".png") &&
                !path.Contains(".jpg"))
            {
                var usuario = context.Session.GetString("Usuario") ?? "Desconocido";
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "IP no detectada";

                // Evitar registrar rutas de AJAX como /Software/ObtenerAlertas o repetidas
                if (!path.Contains("/obtener") && !path.Contains("/actualizar") && !path.Contains("/escanear"))
                {
                    var historial = new HistorialAccion
                    {
                        Fecha = DateTime.Now,
                        Usuario = usuario,
                        IP = ip,
                        Accion = $"Navegó a {path}"
                    };

                    db.HistorialAcciones.Add(historial);
                    db.SaveChanges();
                }
            }

            await _next(context);
        }
    }
}

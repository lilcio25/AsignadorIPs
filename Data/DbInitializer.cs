using ASIGNADORIPS.Models;
using System.Linq;

namespace ASIGNADORIPS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Si ya existen usuarios, no hace nada
            if (context.Usuarios.Any()) return;

            var admin = new Usuario
            {
                NombreUsuario = "admin",
                Contraseña = "admin123", // para pruebas, ideal encriptar luego
                Rol = "admin"
            };

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }
    }
}

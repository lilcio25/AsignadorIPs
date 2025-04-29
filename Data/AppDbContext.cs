using Microsoft.EntityFrameworkCore;
using ASIGNADORIPS.Models;

namespace ASIGNADORIPS.Data
{
    public class AppDbContext : DbContext
    {
#pragma warning disable CS8618
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
#pragma warning restore CS8618

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } // sigue siendo usado para el login
        public DbSet<Software> Softwares { get; set; }
        public DbSet<UsuarioSoftware> UsuarioSoftwares { get; set; }
        public DbSet<Personal> Personal { get; set; } // Nuevo DbSet
        public DbSet<ConfiguracionCorreo> ConfiguracionCorreo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsuarioSoftware>()
                .HasKey(us => us.Id);

            modelBuilder.Entity<UsuarioSoftware>()
                .HasOne(us => us.Software)
                .WithMany()
                .HasForeignKey(us => us.SoftwareId);

            modelBuilder.Entity<UsuarioSoftware>()
                .HasOne(us => us.Personal)  // Asociar correctamente con la propiedad Personal
                .WithMany()
                .HasForeignKey(us => us.PersonalId);
        }
    }
}


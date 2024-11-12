using Microsoft.EntityFrameworkCore;
using Vanessa.Models;

namespace Vanessa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }
        public DbSet<Semillero> Semilleros { get; set; } // Agregado para Semillero
        public DbSet<Proyecto> Proyectos { get; set; }   // Agregado para Proyecto
        public DbSet<Publicacion> Publicaciones { get; set; } // Agregado para Publicacion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla intermedia UsuarioPermiso
            modelBuilder.Entity<UsuarioPermiso>()
                .HasKey(up => new { up.UsuarioId, up.PermisoId }); // Clave primaria compuesta

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Usuario)
                .WithMany(u => u.UsuarioPermisos)
                .HasForeignKey(up => up.UsuarioId);

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Permiso)
                .WithMany(p => p.UsuarioPermisos)
                .HasForeignKey(up => up.PermisoId);

            // Relación uno a muchos entre Usuario y Semillero
            modelBuilder.Entity<Semillero>()
                .HasOne(s => s.Usuario)  // Un Semillero tiene un Usuario
                .WithMany(u => u.Semilleros)  // Un Usuario puede tener muchos Semilleros
                .HasForeignKey(s => s.UsuarioId);

            // Relación uno a muchos entre Semillero y Proyecto
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Semillero)  // Un Proyecto tiene un Semillero
                .WithMany(s => s.Proyectos)  // Un Semillero puede tener muchos Proyectos
                .HasForeignKey(p => p.SemilleroId);

            // Relación uno a muchos entre Usuario y Publicacion
            modelBuilder.Entity<Publicacion>()
                .HasOne(pub => pub.Usuario)  // Una Publicación tiene un Usuario
                .WithMany(u => u.Publicaciones)  // Un Usuario puede tener muchas Publicaciones
                .HasForeignKey(pub => pub.UsuarioId);
        }
    }
}

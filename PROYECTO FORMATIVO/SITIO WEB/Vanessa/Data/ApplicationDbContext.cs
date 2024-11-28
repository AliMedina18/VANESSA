using Microsoft.EntityFrameworkCore;
using Vanessa.Models;

namespace Vanessa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Definición de DbSet para las entidades
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }
        public DbSet<Semillero> Semilleros { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<Publicacion> Publicaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla intermedia UsuarioPermiso
            ConfigureUsuarioPermiso(modelBuilder);

         
        }

        // Método para configurar la relación UsuarioPermiso
        private void ConfigureUsuarioPermiso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioPermiso>()
                .HasKey(up => new { up.UsuarioId, up.PermisoId }); // Clave primaria compuesta

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Usuario)
                .WithMany(u => u.UsuarioPermisos)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);  // Definir comportamiento al eliminar

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Permiso)
                .WithMany(p => p.UsuarioPermisos)
                .HasForeignKey(up => up.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);  // Definir comportamiento al eliminar
        }

        
    }
}
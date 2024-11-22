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

            // Configuración de las relaciones entre Usuario, Semillero, Proyecto y Publicación
            ConfigureSemilleroRelations(modelBuilder);
            ConfigureProyectoRelations(modelBuilder);
            ConfigurePublicacionRelations(modelBuilder);
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

        // Configurar las relaciones entre Semillero y Usuario
        private void ConfigureSemilleroRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Semillero>()
                .HasOne(s => s.Usuario)  // Un Semillero tiene un Usuario
                .WithMany(u => u.Semilleros)  // Un Usuario puede tener muchos Semilleros
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);  // Al eliminar Usuario, no eliminar Semillero, pero establecer UsuarioId a null
        }

        // Configurar la relación entre Semillero y Proyecto
        private void ConfigureProyectoRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Semillero)  // Un Proyecto tiene un Semillero
                .WithMany(s => s.Proyectos)  // Un Semillero puede tener muchos Proyectos
                .HasForeignKey(p => p.SemilleroId)
                .OnDelete(DeleteBehavior.Restrict);  // Evitar la eliminación en cascada
        }

        // Configurar la relación entre Publicación y Usuario
        private void ConfigurePublicacionRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Publicacion>()
                .HasOne(pub => pub.Usuario)  // Una Publicación tiene un Usuario
                .WithMany(u => u.Publicaciones)  // Un Usuario puede tener muchas Publicaciones
                .HasForeignKey(pub => pub.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);  // Al eliminar Usuario, eliminar también las Publicaciones asociadas
        }
    }
}
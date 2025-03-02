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
            ConfigurePublicacionRelations(modelBuilder);
            ConfigureSemilleroProyectoRelations(modelBuilder);
            ConfigureUsuarioProyectoRelations(modelBuilder); // Nueva relación añadida
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

        // Configurar la relación entre Publicación y Usuario
        private void ConfigurePublicacionRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Publicacion>()
                .HasOne(pub => pub.Usuario)  // Una Publicación tiene un Usuario
                .WithMany(u => u.Publicaciones)  // Un Usuario puede tener muchas Publicaciones
                .HasForeignKey(pub => pub.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);  // Al eliminar Usuario, eliminar también las Publicaciones asociadas
        }

        // Configurar la relación entre Semillero y Proyecto (Uno a Muchos)
        private void ConfigureSemilleroProyectoRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Semillero)  // Un Proyecto pertenece a un Semillero
                .WithMany(s => s.Proyectos)  // Un Semillero tiene muchos Proyectos
                .HasForeignKey(p => p.SemilleroId)
                .OnDelete(DeleteBehavior.Cascade);  // Si se elimina un Semillero, se eliminan sus Proyectos
        }

        // Configurar la relación entre Usuario y Proyecto (Uno a Muchos)
        private void ConfigureUsuarioProyectoRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Usuario)  // Un Proyecto pertenece a un Usuario
                .WithMany(u => u.Proyectos)  // Un Usuario puede tener muchos Proyectos
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);  // Si se elimina un Usuario, se eliminan sus Proyectos
        }
    }
}

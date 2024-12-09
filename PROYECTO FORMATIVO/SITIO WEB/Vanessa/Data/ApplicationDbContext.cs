using Microsoft.EntityFrameworkCore;
using Vanessa.Models; // Asegúrate de reemplazar con los namespaces correctos

namespace Vanessa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
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

            // Configuración de UsuarioPermiso
            ConfigureUsuarioPermiso(modelBuilder);


            // Configuración específica de Publicaciones
            ConfigurePublicaciones(modelBuilder);
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



        // Configuración específica para Publicaciones
        private void ConfigurePublicaciones(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Publicacion>(entity =>
            {
                // Configurar clave primaria
                entity.HasKey(col => col.Id_Publicacion);

                // Configurar columna Id_Publicacion como auto-incremental
                entity.Property(col => col.Id_Publicacion)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

                // Configurar propiedades requeridas
                entity.Property(col => col.NombrePublicacion)
                    .IsRequired()
                    .HasMaxLength(100); // Longitud máxima

                entity.Property(col => col.ContenidoPublicacion)
                    .IsRequired();

                entity.Property(col => col.FechaPublicacion)
                    .IsRequired();

                entity.Property(col => col.ImagenPublicacion)
                    .IsRequired();

                // Configurar propiedades opcionales
                entity.Property(col => col.TipoPublicacion)
                    .HasMaxLength(50);

                entity.Property(col => col.LugarPublicacion)
                    .HasMaxLength(100);

                entity.Property(col => col.ActividadesPublicacion)
                    .HasMaxLength(200);

                // Configurar nombre de la tabla en la base de datos (opcional)
                entity.ToTable("Publicaciones");
            });
        }
    }
}

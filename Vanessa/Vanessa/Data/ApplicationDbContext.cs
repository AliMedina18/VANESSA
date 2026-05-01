using Microsoft.EntityFrameworkCore;
using Vanessa.Models;

namespace Vanessa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Tablas principales
        public DbSet<Usuario>       Usuarios       { get; set; }
        public DbSet<Rol>           Roles          { get; set; }
        public DbSet<Permiso>       Permisos       { get; set; }

        // Tablas de auditoría y datos transitorios
        public DbSet<UsuarioAuditoria> UsuarioAuditorias { get; set; }

        // Tablas de relaciones muchos-a-muchos
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }

        // Contenido
        public DbSet<Semillero>     Semilleros     { get; set; }
        public DbSet<Proyecto>      Proyectos      { get; set; }
        public DbSet<ProyectoMiembro> ProyectoMiembros { get; set; }
        public DbSet<Publicacion>   Publicaciones  { get; set; }
        public DbSet<TiposPublicacion> TiposPublicacion { get; set; }
        public DbSet<PublicacionAdjunto> PublicacionAdjuntos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─── CONFIGURACIÓN: UsuarioPermiso (relación M:M) ───────────────────────────
            modelBuilder.Entity<UsuarioPermiso>()
                .HasKey(up => new { up.UsuarioId, up.PermisoId });

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Usuario)
                .WithMany(u => u.UsuarioPermisos)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioPermiso>()
                .HasOne(up => up.Permiso)
                .WithMany(p => p.UsuarioPermisos)
                .HasForeignKey(up => up.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ─── CONFIGURACIÓN: RolPermiso (relación M:M) ──────────────────────────────
            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.Permisos)
                .HasForeignKey(rp => rp.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ─── CONFIGURACIÓN: UsuarioAuditoria (relación 1:1) ──────────────────────
            modelBuilder.Entity<UsuarioAuditoria>()
                .HasOne(ua => ua.Usuario)
                .WithOne(u => u.Auditoria)
                .HasForeignKey<UsuarioAuditoria>(ua => ua.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ─── CONFIGURACIÓN: Publicacion → TiposPublicacion ──────────────────────
            modelBuilder.Entity<Publicacion>()
                .HasOne(p => p.TipoPublicacion)
                .WithMany(tp => tp.Publicaciones)
                .HasForeignKey(p => p.TipoPublicacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── CONFIGURACIÓN: Publicacion → Usuario ─────────────────────────────
            modelBuilder.Entity<Publicacion>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Publicaciones)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ─── CONFIGURACIÓN: PublicacionAdjunto → Publicacion ───────────────────
            modelBuilder.Entity<PublicacionAdjunto>()
                .HasOne(pa => pa.Publicacion)
                .WithMany(p => p.Adjuntos)
                .HasForeignKey(pa => pa.PublicacionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ─── CONFIGURACIÓN: Proyecto → Semillero ──────────────────────────────
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Semillero)
                .WithMany(s => s.Proyectos)
                .HasForeignKey(p => p.SemilleroId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── CONFIGURACIÓN: Proyecto → Usuario (Coordinador) ───────────────────
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.UsuarioCoordenador)
                .WithMany(u => u.ProyectosCoordinados)
                .HasForeignKey(p => p.UsuarioCoordenadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── CONFIGURACIÓN: ProyectoMiembro (relación M:M) ─────────────────────
            modelBuilder.Entity<ProyectoMiembro>()
                .HasOne(pm => pm.Proyecto)
                .WithMany(p => p.Miembros)
                .HasForeignKey(pm => pm.ProyectoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProyectoMiembro>()
                .HasOne(pm => pm.Usuario)
                .WithMany(u => u.MiembrosiaProyectos)
                .HasForeignKey(pm => pm.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restricción UNIQUE: Un usuario no puede tener dos roles en el mismo proyecto
            modelBuilder.Entity<ProyectoMiembro>()
                .HasIndex(pm => new { pm.ProyectoId, pm.UsuarioId })
                .IsUnique();

            // ─── CONFIGURACIÓN: Semillero → Usuario (Coordinador) ──────────────────
            modelBuilder.Entity<Semillero>()
                .HasOne(s => s.UsuarioCoordinador)
                .WithMany(u => u.SemillerosCoordinados)
                .HasForeignKey(s => s.UsuarioCoordinadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── ÍNDICES PARA RENDIMIENTO ────────────────────────────────────────
            // Usuario - búsquedas por email y documento
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Documento)
                .IsUnique();

            // Publicacion - búsquedas frecuentes
            modelBuilder.Entity<Publicacion>()
                .HasIndex(p => p.TipoPublicacionId);

            modelBuilder.Entity<Publicacion>()
                .HasIndex(p => p.UsuarioId);

            modelBuilder.Entity<Publicacion>()
                .HasIndex(p => p.FechaPublicacion);

            // Proyecto - búsquedas comunes
            modelBuilder.Entity<Proyecto>()
                .HasIndex(p => p.SemilleroId);

            modelBuilder.Entity<Proyecto>()
                .HasIndex(p => p.UsuarioCoordenadorId);

            modelBuilder.Entity<Proyecto>()
                .HasIndex(p => new { p.SemilleroId, p.Estado });

            // Semillero - búsquedas
            modelBuilder.Entity<Semillero>()
                .HasIndex(s => s.Nombre);

            modelBuilder.Entity<Semillero>()
                .HasIndex(s => s.UsuarioCoordinadorId);

            modelBuilder.Entity<Semillero>()
                .HasIndex(s => new { s.Activo, s.FechaCreacion });

            // ProyectoMiembro - búsquedas de participación
            modelBuilder.Entity<ProyectoMiembro>()
                .HasIndex(pm => new { pm.ProyectoId, pm.RolMiembro });

            modelBuilder.Entity<ProyectoMiembro>()
                .HasIndex(pm => pm.UsuarioId);
        }
    }
}


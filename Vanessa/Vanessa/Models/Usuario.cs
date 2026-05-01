using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Usuario - refactorizado para 3FN.
    /// Almacena SOLO datos persistentes del usuario.
    /// Datos transitorios (tokens, auditoría de login) → UsuarioAuditoria
    /// </summary>
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Número de documento de identidad (BIGINT para documentos grandes).
        /// </summary>
        [Required(ErrorMessage = "El documento es obligatorio.")]
        [Range(1_000_000, 9_999_999_999, ErrorMessage = "Ingrese un número de documento válido.")]
        public long Documento { get; set; }

        /// <summary>
        /// Correo único del usuario (RFC 5321: máximo 254 caracteres).
        /// </summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(254, ErrorMessage = "El correo no puede superar los 254 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]   // hash PBKDF2 con margen
        public string Contraseña { get; set; } = string.Empty;

        /// <summary>
        /// Solo se usa en formularios; nunca se persiste en BD.
        /// </summary>
        [NotMapped]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContraseña { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario (relación 1:N con Rol)
        /// </summary>
        [ForeignKey("Rol")]
        public int RolId { get; set; }
        public Rol? Rol { get; set; }

        /// <summary>
        /// Permisos adicionales/excepcionales del usuario (relación M:M)
        /// </summary>
        public ICollection<UsuarioPermiso>? UsuarioPermisos { get; set; }

        /// <summary>
        /// Estado activo del usuario
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha (UTC) en que el usuario fue marcado como inactivo (soft-delete)
        /// </summary>
        public DateTime? FechaInactivo { get; set; }

        /// <summary>
        /// Fecha (UTC) de creación del usuario. Auditoría.
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha (UTC) de última actualización de datos del usuario. Auditoría.
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // ─── RELACIONES DE NAVEGACIÓN ──────────────────────────────────
        /// <summary>
        /// Datos transitorios y de auditoría (tokens, login) - Relación 1:1
        /// </summary>
        public UsuarioAuditoria? Auditoria { get; set; }

        /// <summary>
        /// Semilleros coordinados por este usuario
        /// </summary>
        public ICollection<Semillero> SemillerosCoordinados { get; set; } = new List<Semillero>();

        /// <summary>
        /// Publicaciones creadas por este usuario
        /// </summary>
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();

        /// <summary>
        /// Proyectos coordinados por este usuario
        /// </summary>
        public ICollection<Proyecto> ProyectosCoordinados { get; set; } = new List<Proyecto>();

        /// <summary>
        /// Membresía en proyectos (rol en cada proyecto)
        /// </summary>
        public ICollection<ProyectoMiembro> MiembrosiaProyectos { get; set; } = new List<ProyectoMiembro>();
    }
}


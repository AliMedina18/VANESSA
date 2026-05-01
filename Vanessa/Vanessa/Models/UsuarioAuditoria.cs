using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Almacena datos transitorios y de auditoría de Usuario.
    /// Separado de Usuario para mantener la tabla principal esbelta.
    /// </summary>
    public class UsuarioAuditoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// FK usuario (relación 1:1)
        /// </summary>
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Token para recuperación de contraseña. Se genera al solicitar reset.
        /// </summary>
        [StringLength(500)]
        public string? TokenRecuperacion { get; set; }

        /// <summary>
        /// Fecha UTC de expiración del token de recuperación.
        /// </summary>
        public DateTime? TokenExpiracion { get; set; }

        /// <summary>
        /// Fecha en que se generó el token de recuperación (auditoría).
        /// </summary>
        public DateTime? FechaCreacionToken { get; set; }

        /// <summary>
        /// Último login registrado del usuario.
        /// </summary>
        public DateTime? FechaUltimoLogin { get; set; }

        /// <summary>
        /// Actualización general de auditoría.
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}

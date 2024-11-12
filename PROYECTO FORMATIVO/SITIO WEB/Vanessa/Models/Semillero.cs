using System;
using System.Collections.Generic;

namespace Vanessa.Models
{
    public class Semillero
    {
        public int? Id { get; set; }

        public string? Nombre { get; set; }

        public string? Imagen { get; set; }

        public string? Area { get; set; }

        public string? Descripcion { get; set; }

        // Llave foránea hacia Usuario
        public int? UsuarioId { get; set; }

        // Relación con Usuario (Muchos Semilleros pueden tener un Usuario)
        public Usuario? Usuario { get; set; }

        // Relación uno a muchos con Proyecto (Un Semillero tiene varios Proyectos)
        public ICollection<Proyecto>? Proyectos { get; set; } = new List<Proyecto>();
    }

}

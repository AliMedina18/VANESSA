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

    }

}

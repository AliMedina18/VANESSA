using System;

namespace Vanessa.Models
{
    public class Proyecto
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public string? DocumentoProyecto { get; set; }

        public string? EquiposInvestigacion { get; set; }

        public DateTime FechaInicio { get; set; }

    }

}

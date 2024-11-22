using System;

namespace Vanessa.Models
{
    public class Proyecto
    {
        public int Id { get; set; }

        public required string Nombre { get; set; }

        public required string DocumentoProyecto { get; set; }

        public required string EquiposInvestigacion { get; set; }

        public DateTime FechaInicio { get; set; }

        public int SemilleroId { get; set; }
        public required Semillero Semillero { get; set; }
    }

}

namespace Vanessa.Models.Enums
{
    /// <summary>
    /// Ciclo de vida de un proyecto de investigación.
    /// Almacenado como integer en la base de datos.
    /// </summary>
    public enum EstadoProyecto
    {
        Activo    = 0,
        Finalizado = 1,
        Archivado  = 2
    }
}

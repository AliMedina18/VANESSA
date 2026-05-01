namespace Vanessa.Models.Enums
{
    /// <summary>
    /// Tipo de publicación. El valor string se almacena en BD para compatibilidad
    /// con el select HTML existente ("articulo", "evento", "noticia").
    /// </summary>
    public static class TiposPublicacion
    {
        public const string Articulo = "articulo";
        public const string Evento   = "evento";
        public const string Noticia  = "noticia";

        public static readonly IReadOnlyList<string> Todos =
            new[] { Articulo, Evento, Noticia };

        public static bool EsValido(string? tipo) =>
            tipo != null && Todos.Contains(tipo);
    }
}

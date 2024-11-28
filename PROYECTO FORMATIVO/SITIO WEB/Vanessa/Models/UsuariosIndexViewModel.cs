namespace Vanessa.Models
{
    public class UsuariosIndexViewModel
    {
        public List<Usuario> Usuarios { get; set; }  // Lista de usuarios a mostrar
        public int PageNumber { get; set; }          // Número de la página actual
        public int TotalPages { get; set; }          // Número total de páginas
        public int PageSize { get; set; }            // Tamaño de página (cuántos usuarios por página)
    }


}

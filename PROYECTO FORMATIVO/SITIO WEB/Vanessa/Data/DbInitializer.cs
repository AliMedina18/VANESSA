using Vanessa.Models;
using Vanessa.Services;
using Vanessa.Data;
using System.Linq;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context, AuthService authService)
    {
        // Asegúrate de que la base de datos esté creada
        context.Database.EnsureCreated();

        // Verifica si ya hay roles y permisos en la base de datos
        if (context.Roles.Any())
        {
            return; // La base de datos ya ha sido inicializada
        }

        // =======================
        // 1. Crear Roles
        // =======================
        var roles = new List<Rol>
        {
            new Rol { Nombre = "Coordinador" },
            new Rol { Nombre = "Cliente" },
            new Rol { Nombre = "Docente" },
            new Rol { Nombre = "Estudiante" }
        };
        context.Roles.AddRange(roles);
        context.SaveChanges();

        // Obtener el rol Coordinador
        var rolCoordinador = roles.FirstOrDefault(r => r.Nombre == "Coordinador");

     
        // 2. Crear Permisos

        var permisos = new List<Permiso>
        {
            new Permiso { Nombre = "VerUsuarios", Descripcion = "Puede ver la lista de usuarios" },
            new Permiso { Nombre = "CrearUsuarios", Descripcion = "Puede crear nuevos usuarios" },
            new Permiso { Nombre = "EditarUsuarios", Descripcion = "Puede editar usuarios existentes" },
            new Permiso { Nombre = "EliminarUsuarios", Descripcion = "Puede eliminar usuarios" },
            new Permiso { Nombre = "VerPerfilPropio", Descripcion = "Puede ver su propio perfil" },
            new Permiso { Nombre = "ActualizarPerfilPropio", Descripcion = "Puede actualizar su propio perfil" },
            new Permiso { Nombre = "EliminarPerfilPropioConPermiso", Descripcion = "Puede eliminar su cuenta con aprobación del coordinador" },
        };
        context.Permisos.AddRange(permisos);
        context.SaveChanges();

     
        // 3. Crear Usuario Coordinador

        if (!context.Usuarios.Any(u => u.Correo == "alivalmedina2006@gmail.com"))
        {
            var usuarioCoordinador = new Usuario
            {
                Nombre = "Alicia",
                Documento = 12345678,
                Correo = "alivalmedina2006@gmail.com",
                Contraseña = authService.ConvertirContraseña("Ali*02102006"),
                RolId = rolCoordinador.Id,
                Activo = true
            };

            context.Usuarios.Add(usuarioCoordinador);
            context.SaveChanges();

            // Asignar todos los permisos al Coordinador
            var permisosCoordinador = context.Permisos.ToList();
            foreach (var permiso in permisosCoordinador)
            {
                context.UsuarioPermisos.Add(new UsuarioPermiso
                {
                    UsuarioId = usuarioCoordinador.Id,
                    PermisoId = permiso.Id
                });
            }
            context.SaveChanges();
        }

        // =======================
        // 4. Crear Usuarios Regulares (Clientes, Docentes, etc.) y asignar permisos
        // =======================
        if (!context.Usuarios.Any(u => u.Correo == "usuario@cliente.com"))
        {
            var usuarioRegular = new Usuario
            {
                Nombre = "Cliente Usuario",
                Documento = 87654321,
                Correo = "usuario@cliente.com",
                Contraseña = authService.ConvertirContraseña("Pass*1234"),
                RolId = roles.FirstOrDefault(r => r.Nombre == "Cliente").Id,
                Activo = true
            };

            context.Usuarios.Add(usuarioRegular);
            context.SaveChanges();

            authService.AsignarPermisosParaUsuario(context, usuarioRegular);
        }

        context.SaveChanges();
    }
}

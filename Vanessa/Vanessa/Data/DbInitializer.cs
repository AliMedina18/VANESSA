using Vanessa.Models;
using Vanessa.Services;
using Vanessa.Data;
using System.Linq;
using System.Collections.Generic;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context, AuthService authService)
    {
        context.Database.EnsureCreated();

        if (context.Roles.Any() && context.Permisos.Any())
        {
            return;
        }

        CrearRoles(context);
        CrearPermisos(context);
        CrearUsuarioCoordinador(context, authService);
        CrearUsuariosRegulares(context, authService);
    }

    private static void CrearRoles(ApplicationDbContext context)
    {
        var roles = new List<Rol>
        {
            new Rol { Nombre = "Coordinador" },
            new Rol { Nombre = "Cliente" },
            new Rol { Nombre = "Docente" },
            new Rol { Nombre = "Estudiante" }
        };
        context.Roles.AddRange(roles);
        context.SaveChanges();
    }

    private static void CrearPermisos(ApplicationDbContext context)
    {
        var permisos = new List<Permiso>
        {
            new Permiso { Nombre = "VerUsuarios", Descripcion = "Puede ver la lista de usuarios" },
            new Permiso { Nombre = "CrearUsuarios", Descripcion = "Puede crear nuevos usuarios" },
            new Permiso { Nombre = "EditarUsuarios", Descripcion = "Puede editar usuarios existentes" },
            new Permiso { Nombre = "EliminarUsuarios", Descripcion = "Puede eliminar usuarios" },
            new Permiso { Nombre = "VerPerfilPropio", Descripcion = "Puede ver su propio perfil" },
            new Permiso { Nombre = "ActualizarPerfilPropio", Descripcion = "Puede actualizar su propio perfil" },
            new Permiso { Nombre = "EliminarPerfilPropioConPermiso", Descripcion = "Puede eliminar su cuenta con aprobación del coordinador" },
            new Permiso { Nombre = "SuperUsuario", Descripcion = "Acceso total a todas las funcionalidades" },
            new Permiso { Nombre = "CrearProyectos", Descripcion = "Puede crear proyectos" },
            new Permiso { Nombre = "EditarProyectos", Descripcion = "Puede editar proyectos" },
            new Permiso { Nombre = "EliminarProyectos", Descripcion = "Puede eliminar proyectos con permiso del coordinador" },
            new Permiso { Nombre = "EditarSemillero", Descripcion = "Puede editar semillero con permiso del coordinador" },
            new Permiso { Nombre = "VerProyectos", Descripcion = "Puede ver proyectos" },
           
            new Permiso { Nombre = "EditarPublicaciones", Descripcion = "Puede editar cualquier publicación" }
        };
        context.Permisos.AddRange(permisos);
        context.SaveChanges();
    }

    private static void CrearUsuarioCoordinador(ApplicationDbContext context, AuthService authService)
    {
        var rolCoordinador = context.Roles.FirstOrDefault(r => r.Nombre == "Coordinador");
        if (rolCoordinador == null) return;

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

            AsignarPermisosAUsuario(context, usuarioCoordinador, true);
        }
    }

    private static void CrearUsuariosRegulares(ApplicationDbContext context, AuthService authService)
    {
        CrearUsuarioPorRol(context, authService, "Cliente", "cliente@correo.com", "Pass*1234", new List<string> { "VerProyectos" });
        CrearUsuarioPorRol(context, authService, "Docente", "docente@correo.com", "Docente*1234", new List<string> { "CrearProyectos", "EditarProyectos", "VerProyectos" });
        CrearUsuarioPorRol(context, authService, "Estudiante", "estudiante@correo.com", "Estudiante*1234", new List<string> { "CrearProyectos", "VerProyectos"});
    }

    private static void CrearUsuarioPorRol(ApplicationDbContext context, AuthService authService, string rolNombre, string correo, string password, List<string> permisos)
    {
        if (!context.Usuarios.Any(u => u.Correo == correo))
        {
            var rol = context.Roles.FirstOrDefault(r => r.Nombre == rolNombre);
            if (rol != null)
            {
                var usuario = new Usuario
                {
                    Nombre = rolNombre + " Usuario",
                    Documento = new Random().Next(10000000, 99999999),
                    Correo = correo,
                    Contraseña = authService.ConvertirContraseña(password),
                    RolId = rol.Id,
                    Activo = true
                };
                context.Usuarios.Add(usuario);
                context.SaveChanges();

                AsignarPermisosAUsuario(context, usuario, false, permisos);
            }
        }
    }

    private static void AsignarPermisosAUsuario(ApplicationDbContext context, Usuario usuario, bool esSuperUsuario = false, List<string> permisosEspecificos = null)
    {
        List<Permiso> permisos;
        if (esSuperUsuario)
        {
            permisos = context.Permisos.ToList();
        }
        else
        {
            permisos = context.Permisos.Where(p => permisosEspecificos.Contains(p.Nombre)).ToList();
        }

        foreach (var permiso in permisos)
        {
            context.UsuarioPermisos.Add(new UsuarioPermiso
            {
                UsuarioId = usuario.Id,
                PermisoId = permiso.Id
            });
        }
        context.SaveChanges();
    }
}

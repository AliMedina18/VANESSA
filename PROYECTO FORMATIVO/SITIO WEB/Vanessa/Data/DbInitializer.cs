using Vanessa.Models;
using Vanessa.Services;
using Vanessa.Data;
using System.Linq;
using System.Collections.Generic;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context, AuthService authService)
    {
        // Asegúrate de que la base de datos esté creada
        context.Database.EnsureCreated();

        // Si ya existen roles y permisos, termina la ejecución
        if (context.Roles.Any() && context.Permisos.Any())
        {
            return; // La base de datos ya ha sido inicializada
        }

        // =======================
        // 1. Crear Roles
        // =======================
        CrearRoles(context);

        // =======================
        // 2. Crear Permisos
        // =======================
        CrearPermisos(context);

        // =======================
        // 3. Crear Usuario Coordinador
        // =======================
        CrearUsuarioCoordinador(context, authService);

        // =======================
        // 4. Crear Usuarios Regulares
        // =======================
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
        };
        context.Permisos.AddRange(permisos);
        context.SaveChanges();
    }

    private static void CrearUsuarioCoordinador(ApplicationDbContext context, AuthService authService)
    {
        // Obtener el rol Coordinador
        var rolCoordinador = context.Roles.FirstOrDefault(r => r.Nombre == "Coordinador");
        if (rolCoordinador == null) return; // Si no existe el rol Coordinador, salir

        // Crear usuario Coordinador
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
            AsignarPermisosAUsuario(context, usuarioCoordinador);
        }
    }

    private static void CrearUsuariosRegulares(ApplicationDbContext context, AuthService authService)
    {
        // Crear usuario Cliente
        if (!context.Usuarios.Any(u => u.Correo == "usuario@cliente.com"))
        {
            var rolCliente = context.Roles.FirstOrDefault(r => r.Nombre == "Cliente");
            if (rolCliente != null)
            {
                var usuarioCliente = new Usuario
                {
                    Nombre = "Cliente Usuario",
                    Documento = 87654321,
                    Correo = "usuario@cliente.com",
                    Contraseña = authService.ConvertirContraseña("Pass*1234"),
                    RolId = rolCliente.Id,
                    Activo = true
                };

                context.Usuarios.Add(usuarioCliente);
                context.SaveChanges();

                authService.AsignarPermisosParaUsuario(context, usuarioCliente);
            }
        }

        // Se pueden agregar más usuarios regulares (Docentes, Estudiantes) aquí, si es necesario.
    }

    private static void AsignarPermisosAUsuario(ApplicationDbContext context, Usuario usuario)
    {
        var permisos = context.Permisos.ToList();
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

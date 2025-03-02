using System.Security.Cryptography;
using Vanessa.Data;
using Vanessa.Models;
using Vanessa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using PdfSharp.Charting;

var builder = WebApplication.CreateBuilder(args);

// Configuración para la autenticación con cookies y manejo de acceso denegado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Semillero/AccessDenied"; // Redirigir a una acción personalizada en SemilleroController
});


// Agregar servicios al contenedor
builder.Services.AddControllersWithViews();

// **Registrar IHttpContextAccessor para usarlo en vistas y controladores**
builder.Services.AddHttpContextAccessor();

// Configuración de la cadena de conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar AuthService para inyección de dependencias
builder.Services.AddScoped<AuthService>();

// Configurar servicios EmailService
builder.Services.AddScoped<EmailService>();

// Registrar el servicio RecuperacionService
builder.Services.AddScoped<RecuperacionService>();

// Registrar el servicio para eliminación automática
builder.Services.AddHostedService<EliminarUsuariosInactivosService>();

// Configuración para la autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Middleware para usar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Método para convertir la contraseña a hash seguro
string ConvertirContraseña(string contraseña)
{
    byte[] salt = new byte[128 / 8];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }

    var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: contraseña,
        salt: salt,
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

    return $"{Convert.ToBase64String(salt)}.{hashed}";
}

// Método para inicializar datos en la base de datos
void InicializarDatos(ApplicationDbContext context)
{
    try
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(new List<Rol>
            {
                new Rol { Nombre = "Coordinador" },
                new Rol { Nombre = "Cliente" },
                new Rol { Nombre = "Docente" },
                new Rol { Nombre = "Estudiante" }
            });
            context.SaveChanges();
        }

        if (!context.Usuarios.Any(u => u.Documento == 12345678 || u.Correo == "alivalmedina2006@gmail.com"))
        {
            var rolCoordinador = context.Roles.FirstOrDefault(r => r.Nombre == "Coordinador");
            if (rolCoordinador != null)
            {
                var usuarioPredeterminado = new Usuario
                {
                    Nombre = "Alicia",
                    Documento = 12345678,
                    Correo = "alivalmedina2006@gmail.com",
                    Contraseña = ConvertirContraseña("Ali*02102006"),
                    RolId = rolCoordinador.Id
                };

                context.Usuarios.Add(usuarioPredeterminado);
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
    }
}

// Inicializar datos en la base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    InicializarDatos(context);
}

// Configura el manejo de excepciones y errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware para evitar caché
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");
    await next();
});

// Middleware para sesiones y autenticación
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Configura las rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Registro}/{id?}");

app.Run();

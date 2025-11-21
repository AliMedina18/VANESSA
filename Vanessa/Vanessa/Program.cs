using System.Security.Cryptography;
using Vanessa.Data;
using Vanessa.Models;
using Vanessa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using PdfSharp.Charting;
using Npgsql.EntityFrameworkCore.PostgreSQL; 

var builder = WebApplication.CreateBuilder(args);

// Configuración para la autenticación con cookies y manejo de acceso denegado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Semillero/AccessDenied";
});

// Agregar servicios al contenedor
builder.Services.AddControllersWithViews();

// Registrar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ⭐ CAMBIO IMPORTANTE: usar PostgreSQL en lugar de SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<RecuperacionService>();
builder.Services.AddHostedService<EliminarUsuariosInactivosService>();

// Configuración de autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("ngrok-skip-browser-warning", "true");
    await next();
});

// Método hash
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

// Inicializar datos
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

// Inicializar BD
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    InicializarDatos(context);
}

// Middleware y configuración final
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");
    await next();
});

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Registro}/{id?}");

app.Run();

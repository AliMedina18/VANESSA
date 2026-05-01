using Npgsql;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Repositories;
using Vanessa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── Puerto dinámico para plataformas en la nube ───────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT")
    ?? builder.Configuration["PORT"]
    ?? "8080";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ── Base de datos ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(GetDatabaseConnectionString(builder.Configuration)));

// ── Autenticación por cookies ─────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Semillero/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// ── Sesiones ──────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// ── Repositorios ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepository,    UsuarioRepository>();
builder.Services.AddScoped<IProyectoRepository,   ProyectoRepository>();
builder.Services.AddScoped<ISemilleroRepository,  SemilleroRepository>();
builder.Services.AddScoped<IPublicacionRepository, PublicacionRepository>();

// ── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,    AuthService>();
builder.Services.AddScoped<IEmailService,   EmailService>();
builder.Services.AddScoped<RecuperacionService>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddHostedService<EliminarUsuariosInactivosService>();

// ── Caché de respuestas (solo para peticiones autenticadas) ───────────────────
builder.Services.AddResponseCaching();

var app = builder.Build();

// ── Seed de base de datos ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await init.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones o seed inicial.");
        throw;
    }
}

// ── Middleware de ngrok ───────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("ngrok-skip-browser-warning", "true");
    await next();
});

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // archivos estáticos NO llevan Cache-Control agresivo
app.UseRouting();

// Cache-Control solo para rutas autenticadas (no para static files)
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        ctx.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        ctx.Response.Headers.Append("Pragma", "no-cache");
        ctx.Response.Headers.Append("Expires", "0");
    }
    await next();
});

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

static string GetDatabaseConnectionString(IConfiguration configuration)
{
    var databaseUrl = configuration["DATABASE_URL"];
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = userInfo.ElementAtOrDefault(0) ?? string.Empty,
            Password = userInfo.ElementAtOrDefault(1) ?? string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };
        return builder.ToString();
    }

    return configuration.GetConnectionString("DefaultConnection")
           ?? throw new InvalidOperationException("No se encontró la cadena de conexión. Configure ConnectionStrings__DefaultConnection o DATABASE_URL.");
}

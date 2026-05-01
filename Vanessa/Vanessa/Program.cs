using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Repositories;
using Vanessa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ── Base de datos ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await init.SeedAsync();
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

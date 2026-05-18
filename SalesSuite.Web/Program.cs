using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Infrastructure.Data;
using SalesSuite.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Cadena de conexión ────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada en appsettings.json.");

// ── DbContext con PostgreSQL (Npgsql) ─────────────────────────────────────────
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── ASP.NET Core Identity ─────────────────────────────────────────────────────
// Usa el mismo SalesDbContext para que las tablas de Identity queden en la
// misma base de datos PostgreSQL de Supabase junto con las tablas de ventas.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Requisitos de contraseña relajados para entorno de desarrollo/demo.
    // En producción se recomienda activar todos los requisitos.
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;

    // Bloqueo de cuenta tras intentos fallidos
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<SalesDbContext>()
.AddDefaultTokenProviders();

// Configuración de la cookie de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── Repositorios y Unit of Work (Scoped = un objeto por request HTTP) ─────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── AutoMapper — en v16+ la DI se configura con una acción de configuración ──
builder.Services.AddAutoMapper(cfg =>
    cfg.AddProfile<SalesSuite.Web.Mappings.MappingProfile>());

// ── MVC con Vistas Razor ──────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Sesión (para mensajes de feedback entre redirects) ───────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Data Seeder + Usuario de prueba (background, no bloquea el arranque) ──────
_ = Task.Run(async () =>
{
    await Task.Delay(3000);
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db          = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Seed productos y clientes
        await DataSeeder.SeedAsync(db);

        // Crear usuario de prueba si no existe
        const string testEmail    = "admin@salessuite.com";
        const string testPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(testEmail) is null)
        {
            var user = new IdentityUser { UserName = testEmail, Email = testEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, testPassword);
            if (result.Succeeded)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Usuario de prueba creado: {Email}", testEmail);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("DataSeeder omitido: {Msg}", ex.Message);
    }
});

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// El orden importa: Authentication ANTES que Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data;

/// <summary>
/// DbContext principal de SalesSuite.
///
/// Hereda de IdentityDbContext&lt;IdentityUser&gt; para que ASP.NET Core Identity
/// almacene sus tablas (AspNetUsers, AspNetRoles, etc.) en la misma base de
/// datos PostgreSQL de Supabase, junto con las tablas de negocio.
///
/// La configuración de mapeo se delega a clases IEntityTypeConfiguration
/// separadas, manteniendo este archivo limpio y fácil de mantener.
/// </summary>
public class SalesDbContext : IdentityDbContext<IdentityUser>
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    // ── DbSets de negocio ─────────────────────────────────────────────────────
    public DbSet<Producto> Productos     => Set<Producto>();
    public DbSet<Cliente>  Clientes      => Set<Cliente>();
    public DbSet<Venta>    Ventas        => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica primero las configuraciones de Identity (tablas AspNet*).
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las IEntityTypeConfiguration del ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
    }
}

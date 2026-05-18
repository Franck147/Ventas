using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad Producto.
/// Implementa IEntityTypeConfiguration para mantener cada mapeo en su propio archivo
/// y separar la responsabilidad de la configuración del DbContext.
/// </summary>
public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        // ── Tabla ─────────────────────────────────────────────────────────────
        builder.ToTable("Productos");

        // ── Clave primaria ────────────────────────────────────────────────────
        builder.HasKey(p => p.Id);

        // ── Propiedades escalares ─────────────────────────────────────────────
        builder.Property(p => p.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(p => p.CodigoBarras)
               .HasMaxLength(50);

        // Precisión decimal(18,2): 18 dígitos totales, 2 decimales.
        // Equivale a NUMERIC(18,2) en PostgreSQL.
        builder.Property(p => p.Precio)
               .IsRequired()
               .HasColumnType("numeric(18,2)");

        builder.Property(p => p.Stock)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(p => p.StockMinimo)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(p => p.Activo)
               .IsRequired()
               .HasDefaultValue(true);

        // ── Índices ───────────────────────────────────────────────────────────
        // Índice único en CodigoBarras para búsquedas rápidas en el POS
        // (cuando el código existe; los NULL no participan en la restricción unique).
        builder.HasIndex(p => p.CodigoBarras)
               .IsUnique()
               .HasFilter("\"CodigoBarras\" IS NOT NULL"); // sintaxis PostgreSQL

        // ── Relaciones ────────────────────────────────────────────────────────
        // Un Producto puede estar en muchos DetalleVenta.
        // Restrict: no se puede eliminar un producto que ya tiene ventas registradas.
        builder.HasMany(p => p.Detalles)
               .WithOne(d => d.Producto)
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

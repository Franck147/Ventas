using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad DetalleVenta.
/// </summary>
public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        // ── Tabla ─────────────────────────────────────────────────────────────
        builder.ToTable("DetallesVenta");

        // ── Clave primaria ────────────────────────────────────────────────────
        builder.HasKey(d => d.Id);

        // ── Propiedades escalares ─────────────────────────────────────────────
        builder.Property(d => d.Cantidad)
               .IsRequired();

        // Precio histórico: se registra al momento de la venta y nunca cambia.
        builder.Property(d => d.PrecioUnitario)
               .IsRequired()
               .HasColumnType("numeric(18,2)");

        builder.Property(d => d.Subtotal)
               .IsRequired()
               .HasColumnType("numeric(18,2)");

        // ── Relaciones ────────────────────────────────────────────────────────
        // La relación con Venta (Cascade) se define en VentaConfiguration.
        // La relación con Producto (Restrict) se define en ProductoConfiguration.
        // Solo se declaran aquí las FK para que EF las mapee correctamente.

        builder.HasOne(d => d.Venta)
               .WithMany(v => v.Detalles)
               .HasForeignKey(d => d.VentaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
               .WithMany(p => p.Detalles)
               .HasForeignKey(d => d.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

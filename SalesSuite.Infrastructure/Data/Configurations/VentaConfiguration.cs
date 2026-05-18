using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad Venta.
/// </summary>
public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        // ── Tabla ─────────────────────────────────────────────────────────────
        builder.ToTable("Ventas");

        // ── Clave primaria ────────────────────────────────────────────────────
        builder.HasKey(v => v.Id);

        // ── Propiedades escalares ─────────────────────────────────────────────
        builder.Property(v => v.FechaVenta)
               .IsRequired();

        builder.Property(v => v.Total)
               .IsRequired()
               .HasColumnType("numeric(18,2)");

        // ── Relaciones ────────────────────────────────────────────────────────
        // Relación Venta → DetalleVenta: si se elimina una venta (cabecera),
        // sus detalles se eliminan en cascada (son dependientes del agregado).
        builder.HasMany(v => v.Detalles)
               .WithOne(d => d.Venta)
               .HasForeignKey(d => d.VentaId)
               .OnDelete(DeleteBehavior.Cascade);

        // Nota: la relación Venta → Cliente se configura desde ClienteConfiguration
        // usando HasMany/WithOne para mantener la dirección lógica.
    }
}

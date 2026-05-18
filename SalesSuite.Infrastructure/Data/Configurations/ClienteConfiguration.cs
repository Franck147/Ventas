using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad Cliente.
/// </summary>
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        // ── Tabla ─────────────────────────────────────────────────────────────
        builder.ToTable("Clientes");

        // ── Clave primaria ────────────────────────────────────────────────────
        builder.HasKey(c => c.Id);

        // ── Propiedades escalares ─────────────────────────────────────────────
        builder.Property(c => c.DocumentoIdentidad)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(c => c.Nombre)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Apellido)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Email)
               .HasMaxLength(150);

        builder.Property(c => c.Telefono)
               .HasMaxLength(20);

        // ── Índices ───────────────────────────────────────────────────────────
        // El documento de identidad debe ser único en el sistema.
        builder.HasIndex(c => c.DocumentoIdentidad)
               .IsUnique();

        // ── Relaciones ────────────────────────────────────────────────────────
        // Un Cliente puede tener muchas Ventas.
        // Restrict: no se puede eliminar un cliente que ya tiene ventas registradas.
        builder.HasMany(c => c.Ventas)
               .WithOne(v => v.Cliente)
               .HasForeignKey(v => v.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Domain.Entities;

/// <summary>
/// Representa un producto del catálogo disponible para la venta.
/// Es un agregado raíz: toda operación sobre stock pasa por esta entidad.
/// </summary>
public class Producto
{
    // ── Clave primaria ────────────────────────────────────────────────────────
    public int Id { get; set; }

    // ── Datos de identificación ───────────────────────────────────────────────

    /// <summary>Nombre descriptivo del producto (visible en catálogo y tickets).</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Código de barras EAN-13 o similar. Puede ser nulo si el producto no tiene código.</summary>
    [MaxLength(50, ErrorMessage = "El código de barras no puede superar 50 caracteres.")]
    public string? CodigoBarras { get; set; }

    // ── Datos económicos ──────────────────────────────────────────────────────

    /// <summary>Precio de venta unitario en la moneda local. Debe ser mayor a cero.</summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    // ── Control de inventario ─────────────────────────────────────────────────

    /// <summary>Unidades actualmente disponibles en bodega.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    /// <summary>
    /// Nivel mínimo de stock antes de generar una alerta de reabastecimiento.
    /// Útil para el dashboard de inventario.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; }

    /// <summary>
    /// Indica si el producto está disponible para la venta.
    /// Los productos inactivos no aparecen en el carrito ni en el POS.
    /// </summary>
    public bool Activo { get; set; } = true;

    // ── Propiedad de navegación ───────────────────────────────────────────────

    /// <summary>
    /// Detalles de venta en los que este producto ha participado.
    /// Relación uno-a-muchos con DetalleVenta.
    /// </summary>
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}

using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Domain.Entities;

/// <summary>
/// Línea de detalle de una venta: un producto específico con su cantidad y precio.
/// Pertenece al agregado Venta; no debe modificarse fuera del contexto de su Venta.
/// </summary>
public class DetalleVenta
{
    // ── Clave primaria ────────────────────────────────────────────────────────
    public int Id { get; set; }

    // ── Claves foráneas ───────────────────────────────────────────────────────

    /// <summary>FK hacia la tabla Ventas (cabecera a la que pertenece este detalle).</summary>
    public int VentaId { get; set; }

    /// <summary>FK hacia la tabla Productos.</summary>
    public int ProductoId { get; set; }

    // ── Datos de la línea ─────────────────────────────────────────────────────

    /// <summary>
    /// Número de unidades vendidas de este producto en la transacción.
    /// Mínimo 1: no tiene sentido vender cero unidades.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario en el momento de la venta.
    /// Se persiste aquí para preservar el precio histórico, incluso si el
    /// precio del producto cambia posteriormente.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Resultado de Cantidad × PrecioUnitario.
    /// Calculado en el backend; nunca se recibe del cliente sin validación.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor a 0.")]
    public decimal Subtotal { get; set; }

    // ── Propiedades de navegación ─────────────────────────────────────────────

    /// <summary>Venta a la que pertenece este detalle.</summary>
    public Venta Venta { get; set; } = null!;

    /// <summary>Producto asociado a este detalle (para acceder a nombre, precio actual, etc.).</summary>
    public Producto Producto { get; set; } = null!;
}
